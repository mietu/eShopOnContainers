namespace Microsoft.eShopOnContainers.Payment.API;

/// <summary>
/// 应用程序启动配置类
/// </summary>
public class Startup
{
    /// <summary>
    /// 构造函数，接收配置对象
    /// </summary>
    /// <param name="configuration">应用程序配置</param>
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    /// <summary>
    /// 应用程序配置属性
    /// </summary>
    public IConfiguration Configuration { get; }

    /// <summary>
    /// 配置应用程序服务
    /// 此方法由运行时调用，用于向依赖注入容器添加服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <returns>配置好的服务提供者</returns>
    public IServiceProvider ConfigureServices(IServiceCollection services)
    {
        // 添加自定义健康检查服务
        services.AddCustomHealthCheck(Configuration);
        // 配置支付设置
        services.Configure<PaymentSettings>(Configuration);

        // 注册Application Insights服务
        RegisterAppInsights(services);

        // 根据配置选择消息总线：Azure Service Bus或RabbitMQ
        if (Configuration.GetValue<bool>("AzureServiceBusEnabled"))
        {
            // 如果启用了Azure Service Bus，则注册相关连接服务
            services.AddSingleton<IServiceBusPersisterConnection>(sp =>
            {
                var serviceBusConnectionString = Configuration["EventBusConnection"];
                var subscriptionClientName = Configuration["SubscriptionClientName"];

                return new DefaultServiceBusPersisterConnection(serviceBusConnectionString);
            });
        }
        else
        {
            // 否则注册RabbitMQ连接服务
            services.AddSingleton<IRabbitMQPersistentConnection>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();
                var factory = new ConnectionFactory()
                {
                    HostName = Configuration["EventBusConnection"],
                    DispatchConsumersAsync = true
                };

                // 配置RabbitMQ用户名（如果有）
                if (!string.IsNullOrEmpty(Configuration["EventBusUserName"]))
                {
                    factory.UserName = Configuration["EventBusUserName"];
                }

                // 配置RabbitMQ密码（如果有）
                if (!string.IsNullOrEmpty(Configuration["EventBusPassword"]))
                {
                    factory.Password = Configuration["EventBusPassword"];
                }

                // 设置重试次数，默认为5次
                var retryCount = 5;
                if (!string.IsNullOrEmpty(Configuration["EventBusRetryCount"]))
                {
                    retryCount = int.Parse(Configuration["EventBusRetryCount"]);
                }

                return new DefaultRabbitMQPersistentConnection(factory, logger, retryCount);
            });
        }

        // 注册事件总线相关服务
        RegisterEventBus(services);

        // 使用Autofac作为DI容器
        var container = new ContainerBuilder();
        container.Populate(services);
        return new AutofacServiceProvider(container.Build());
    }

    /// <summary>
    /// 配置HTTP请求处理管道
    /// 此方法由运行时调用，用于配置HTTP请求处理管道
    /// </summary>
    /// <param name="app">应用程序构建器</param>
    /// <param name="loggerFactory">日志工厂</param>
    public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
    {
        // 以下是被注释掉的日志配置
        //loggerFactory.AddAzureWebAppDiagnostics();
        //loggerFactory.AddApplicationInsights(app.ApplicationServices, LogLevel.Trace);

        // 设置应用程序基础路径（如果有）
        var pathBase = Configuration["PATH_BASE"];
        if (!string.IsNullOrEmpty(pathBase))
        {
            app.UsePathBase(pathBase);
        }

        // 配置事件总线订阅
        ConfigureEventBus(app);

        // 配置路由
        app.UseRouting();
        // 配置终结点
        app.UseEndpoints(endpoints =>
        {
            // 配置健康检查终结点
            endpoints.MapHealthChecks("/hc", new HealthCheckOptions()
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
            // 配置活动性检查终结点
            endpoints.MapHealthChecks("/liveness", new HealthCheckOptions
            {
                Predicate = r => r.Name.Contains("self")
            });
        });
    }

    /// <summary>
    /// 注册Application Insights服务
    /// </summary>
    /// <param name="services">服务集合</param>
    private void RegisterAppInsights(IServiceCollection services)
    {
        services.AddApplicationInsightsTelemetry(Configuration);
        services.AddApplicationInsightsKubernetesEnricher();
    }

    /// <summary>
    /// 注册事件总线及相关服务
    /// </summary>
    /// <param name="services">服务集合</param>
    private void RegisterEventBus(IServiceCollection services)
    {
        if (Configuration.GetValue<bool>("AzureServiceBusEnabled"))
        {
            // 注册Azure Service Bus事件总线
            services.AddSingleton<IEventBus, EventBusServiceBus>(sp =>
            {
                var serviceBusPersisterConnection = sp.GetRequiredService<IServiceBusPersisterConnection>();
                var iLifetimeScope = sp.GetRequiredService<ILifetimeScope>();
                var logger = sp.GetRequiredService<ILogger<EventBusServiceBus>>();
                var eventBusSubcriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();
                string subscriptionName = Configuration["SubscriptionClientName"];

                return new EventBusServiceBus(serviceBusPersisterConnection, logger,
                    eventBusSubcriptionsManager, iLifetimeScope, subscriptionName);
            });
        }
        else
        {
            // 注册RabbitMQ事件总线
            services.AddSingleton<IEventBus, EventBusRabbitMQ>(sp =>
            {
                var subscriptionClientName = Configuration["SubscriptionClientName"];
                var rabbitMQPersistentConnection = sp.GetRequiredService<IRabbitMQPersistentConnection>();
                var iLifetimeScope = sp.GetRequiredService<ILifetimeScope>();
                var logger = sp.GetRequiredService<ILogger<EventBusRabbitMQ>>();
                var eventBusSubcriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();

                // 设置重试次数，默认为5次
                var retryCount = 5;
                if (!string.IsNullOrEmpty(Configuration["EventBusRetryCount"]))
                {
                    retryCount = int.Parse(Configuration["EventBusRetryCount"]);
                }

                return new EventBusRabbitMQ(rabbitMQPersistentConnection, logger, iLifetimeScope, eventBusSubcriptionsManager, subscriptionClientName, retryCount);
            });
        }

        // 注册事件处理器
        services.AddTransient<OrderStatusChangedToStockConfirmedIntegrationEventHandler>();
        // 注册事件总线订阅管理器
        services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();
    }

    /// <summary>
    /// 配置事件总线订阅
    /// </summary>
    /// <param name="app">应用程序构建器</param>
    private void ConfigureEventBus(IApplicationBuilder app)
    {
        var eventBus = app.ApplicationServices.GetRequiredService<IEventBus>();
        // 订阅订单状态变更为库存确认的集成事件
        eventBus.Subscribe<OrderStatusChangedToStockConfirmedIntegrationEvent, OrderStatusChangedToStockConfirmedIntegrationEventHandler>();
    }
}

/// <summary>
/// 自定义扩展方法类
/// </summary>
public static class CustomExtensionMethods
{
    /// <summary>
    /// 添加自定义健康检查服务的扩展方法
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">应用程序配置</param>
    /// <returns>配置后的服务集合</returns>
    public static IServiceCollection AddCustomHealthCheck(this IServiceCollection services, IConfiguration configuration)
    {
        var hcBuilder = services.AddHealthChecks();

        // 添加自检健康检查
        hcBuilder.AddCheck("self", () => HealthCheckResult.Healthy());

        // 根据消息总线类型添加不同的健康检查
        if (configuration.GetValue<bool>("AzureServiceBusEnabled"))
        {
            // 添加Azure Service Bus主题健康检查
            hcBuilder
                .AddAzureServiceBusTopic(
                    configuration["EventBusConnection"],
                    topicName: "eshop_event_bus",
                    name: "payment-servicebus-check",
                    tags: new string[] { "servicebus" });
        }
        else
        {
            // 添加RabbitMQ健康检查
            hcBuilder
                .AddRabbitMQ(
                    $"amqp://{configuration["EventBusConnection"]}",
                    name: "payment-rabbitmqbus-check",
                    tags: new string[] { "rabbitmqbus" });
        }

        return services;
    }
}
