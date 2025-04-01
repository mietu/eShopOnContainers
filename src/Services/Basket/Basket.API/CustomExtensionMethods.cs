namespace Microsoft.eShopOnContainers.Services.Basket.API;

public static class CustomExtensionMethods
{
    // 扩展方法：为 IServiceCollection 添加自定义的健康检查服务
    public static IServiceCollection AddCustomHealthCheck(this IServiceCollection services, IConfiguration configuration)
    {
        // 初始化健康检查构建器
        var hcBuilder = services.AddHealthChecks();

        // 添加一个简单的健康检查，始终返回健康状态
        hcBuilder.AddCheck("self", () => HealthCheckResult.Healthy());

        // 添加 Redis 健康检查
        // 使用配置文件中的 ConnectionString 作为 Redis 连接字符串，
        // 并为该检查指定名称和标签
        hcBuilder.AddRedis(
            configuration["ConnectionString"],
            name: "redis-check",
            tags: new string[] { "redis" });

        // 根据配置决定使用 Azure Service Bus 或 RabbitMQ 来添加消息总线检查
        if (configuration.GetValue<bool>("AzureServiceBusEnabled"))
        {
            // 如果启用了 Azure Service Bus，则添加 Azure Service Bus Topic 健康检查
            hcBuilder.AddAzureServiceBusTopic(
                configuration["EventBusConnection"],
                topicName: "eshop_event_bus",
                name: "basket-servicebus-check",
                tags: new string[] { "servicebus" });
        }
        else
        {
            // 否则，添加 RabbitMQ 健康检查，构造 amqp 连接字符串
            hcBuilder.AddRabbitMQ(
                $"amqp://{configuration["EventBusConnection"]}",
                name: "basket-rabbitmqbus-check",
                tags: new string[] { "rabbitmqbus" });
        }

        // 返回服务集合，以便进行链式调用
        return services;
    }
}
