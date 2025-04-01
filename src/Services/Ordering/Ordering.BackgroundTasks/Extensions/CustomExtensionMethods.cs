using Autofac;
using Microsoft.eShopOnContainers.BuildingBlocks.EventBus;
using Microsoft.eShopOnContainers.BuildingBlocks.EventBus.Abstractions;
using Microsoft.eShopOnContainers.BuildingBlocks.EventBusRabbitMQ;
using Microsoft.eShopOnContainers.BuildingBlocks.EventBusServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Serilog;

namespace Ordering.BackgroundTasks.Extensions
{
    // 自定义扩展方法类，提供添加健康检查、事件总线及 Serilog 配置的扩展方法
    public static class CustomExtensionMethods
    {
        // 添加健康检查功能
        public static IServiceCollection AddCustomHealthCheck(this IServiceCollection services, IConfiguration configuration)
        {
            // 创建健康检查构建器
            var hcBuilder = services.AddHealthChecks();

            // 添加自检健康检查：始终返回健康状态
            hcBuilder.AddCheck("self", () => HealthCheckResult.Healthy());

            // 添加 SQL Server 数据库连接检查
            hcBuilder.AddSqlServer(
                    configuration["ConnectionString"], // 数据库连接字符串
                    name: "OrderingTaskDB-check", // 检查名称
                    tags: new string[] { "orderingtaskdb" }); // 标签

            // 根据配置判断是否启用 Azure Service Bus
            if (configuration.GetValue<bool>("AzureServiceBusEnabled"))
            {
                // 添加 Azure Service Bus 主题检查
                hcBuilder.AddAzureServiceBusTopic(
                        configuration["EventBusConnection"], // Azure Service Bus 连接字符串
                        topicName: "eshop_event_bus", // 主题名称
                        name: "orderingtask-servicebus-check", // 检查名称
                        tags: new string[] { "servicebus" }); // 标签
            }
            else
            {
                // 添加 RabbitMQ 检查
                hcBuilder.AddRabbitMQ(
                        $"amqp://{configuration["EventBusConnection"]}", // RabbitMQ 连接字符串格式
                        name: "orderingtask-rabbitmqbus-check", // 检查名称
                        tags: new string[] { "rabbitmqbus" }); // 标签
            }

            return services;
        }

        // 添加事件总线功能
        public static IServiceCollection AddEventBus(this IServiceCollection services, IConfiguration configuration)
        {
            // 从配置中获取订阅客户端名称
            var subscriptionClientName = configuration["SubscriptionClientName"];

            // 判断是否启用 Azure Service Bus
            if (configuration.GetValue<bool>("AzureServiceBusEnabled"))
            {
                // 添加 Azure Service Bus 持久连接单例
                services.AddSingleton<IServiceBusPersisterConnection>(sp =>
                {
                    var serviceBusConnectionString = configuration["EventBusConnection"];
                    return new DefaultServiceBusPersisterConnection(serviceBusConnectionString);
                });

                // 添加使用 Azure Service Bus 的事件总线实现
                services.AddSingleton<IEventBus, EventBusServiceBus>(sp =>
                {
                    var serviceBusPersisterConnection = sp.GetRequiredService<IServiceBusPersisterConnection>();
                    var iLifetimeScope = sp.GetRequiredService<ILifetimeScope>();
                    var logger = sp.GetRequiredService<ILogger<EventBusServiceBus>>();
                    var eventBusSubcriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();
                    string subscriptionName = configuration["SubscriptionClientName"];

                    return new EventBusServiceBus(serviceBusPersisterConnection, logger, eventBusSubcriptionsManager, iLifetimeScope, subscriptionName);
                });
            }
            else
            {
                // 添加 RabbitMQ 持久连接单例
                services.AddSingleton<IRabbitMQPersistentConnection>(sp =>
                {
                    var logger = sp.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();

                    var factory = new ConnectionFactory()
                    {
                        HostName = configuration["EventBusConnection"], // RabbitMQ 主机地址
                        DispatchConsumersAsync = true // 异步派发消费者消息
                    };

                    // 可选：设置用户名
                    if (!string.IsNullOrEmpty(configuration["EventBusUserName"]))
                    {
                        factory.UserName = configuration["EventBusUserName"];
                    }

                    // 可选：设置密码
                    if (!string.IsNullOrEmpty(configuration["EventBusPassword"]))
                    {
                        factory.Password = configuration["EventBusPassword"];
                    }

                    // 设置重试次数，默认为 5
                    var retryCount = 5;
                    if (!string.IsNullOrEmpty(configuration["EventBusRetryCount"]))
                    {
                        retryCount = int.Parse(configuration["EventBusRetryCount"]);
                    }

                    return new DefaultRabbitMQPersistentConnection(factory, logger, retryCount);
                });

                // 添加使用 RabbitMQ 的事件总线实现
                services.AddSingleton<IEventBus, EventBusRabbitMQ>(sp =>
                {
                    var rabbitMQPersistentConnection = sp.GetRequiredService<IRabbitMQPersistentConnection>();
                    var iLifetimeScope = sp.GetRequiredService<ILifetimeScope>();
                    var logger = sp.GetRequiredService<ILogger<EventBusRabbitMQ>>();
                    var eventBusSubcriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();

                    // 设置重试次数，默认为 5
                    var retryCount = 5;
                    if (!string.IsNullOrEmpty(configuration["EventBusRetryCount"]))
                    {
                        retryCount = int.Parse(configuration["EventBusRetryCount"]);
                    }

                    return new EventBusRabbitMQ(rabbitMQPersistentConnection, logger, iLifetimeScope, eventBusSubcriptionsManager, subscriptionClientName, retryCount);
                });
            }

            // 添加事件总线订阅管理器单例
            services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();

            return services;
        }

        // 使用 Serilog 配置日志记录
        public static ILoggingBuilder UseSerilog(this ILoggingBuilder builder, IConfiguration configuration)
        {
            // 从配置中获取 Seq 和 Logstash 的 URL
            var seqServerUrl = configuration["Serilog:SeqServerUrl"];
            var logstashUrl = configuration["Serilog:LogstashgUrl"];

            // 配置 Serilog 日志记录器
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose() // 设置最低日志级别
                .Enrich.WithProperty("ApplicationContext", Program.AppName) // 添加应用上下文属性
                .Enrich.FromLogContext() // 添加日志上下文信息
                .WriteTo.Console() // 输出到控制台
                .WriteTo.Seq(string.IsNullOrWhiteSpace(seqServerUrl) ? "http://seq" : seqServerUrl) // 输出到 Seq 日志服务器
                .WriteTo.Http(string.IsNullOrWhiteSpace(logstashUrl) ? "http://logstash:8080" : logstashUrl, null) // 输出到 Logstash
                .ReadFrom.Configuration(configuration) // 从配置文件读取其他配置
                .CreateLogger();

            return builder;
        }
    }
}
