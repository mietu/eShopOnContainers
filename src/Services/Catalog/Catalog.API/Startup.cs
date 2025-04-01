namespace Microsoft.eShopOnContainers.Services.Catalog.API;

// Startup 类：用于配置应用程序的服务和中间件管道
public class Startup
{
    // 构造函数，注入配置
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    // 存储配置信息
    public IConfiguration Configuration { get; }

    // 配置服务容器，注册各种服务
    public IServiceProvider ConfigureServices(IServiceCollection services)
    {
        // 注册应用洞察、gRPC、MVC、数据库上下文、选项、集成服务、事件总线、Swagger和健康检查服务
        services.AddAppInsight(Configuration)
            .AddGrpc().Services
            .AddCustomMVC(Configuration)
            .AddCustomDbContext(Configuration)
            .AddCustomOptions(Configuration)
            .AddIntegrationServices(Configuration)
            .AddEventBus(Configuration)
            .AddSwagger(Configuration)
            .AddCustomHealthCheck(Configuration);

        // 使用 Autofac 作为 IoC 容器，将所有服务注册到 Autofac 容器中
        var container = new ContainerBuilder();
        container.Populate(services);

        return new AutofacServiceProvider(container.Build());
    }

    // 配置 HTTP 请求管道
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
    {
        // 日志配置（示例：Azure Web App 和 Application Insights 日志，可根据需要启用）
        //loggerFactory.AddAzureWebAppDiagnostics();
        //loggerFactory.AddApplicationInsights(app.ApplicationServices, LogLevel.Trace);

        // 获取路径基准（如果在配置中指定）
        var pathBase = Configuration["PATH_BASE"];

        // 如果 PATH_BASE 不为空，使用该路径基准，并记录日志
        if (!string.IsNullOrEmpty(pathBase))
        {
            loggerFactory.CreateLogger<Startup>().LogDebug("Using PATH BASE '{pathBase}'", pathBase);
            app.UsePathBase(pathBase);
        }

        // 启用 Swagger 中间件，提供 API 文档
        app.UseSwagger()
            .UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"{(!string.IsNullOrEmpty(pathBase) ? pathBase : string.Empty)}/swagger/v1/swagger.json", "Catalog.API V1");
            });

        // 配置路由和 CORS 策略
        app.UseRouting();
        app.UseCors("CorsPolicy");

        // 配置各个终结点
        app.UseEndpoints(endpoints =>
        {
            // 默认路由和 Web API 控制器
            endpoints.MapDefaultControllerRoute();
            endpoints.MapControllers();

            // 映射 _proto 终结点，返回 Proto 文件内容
            endpoints.MapGet("/_proto/", async ctx =>
            {
                ctx.Response.ContentType = "text/plain";
                using var fs = new FileStream(Path.Combine(env.ContentRootPath, "Proto", "catalog.proto"), FileMode.Open, FileAccess.Read);
                using var sr = new StreamReader(fs);
                while (!sr.EndOfStream)
                {
                    var line = await sr.ReadLineAsync();
                    // 如果行不是开始或结束标志，则写入响应
                    if (line != "/* >>" || line != "<< */")
                    {
                        await ctx.Response.WriteAsync(line);
                    }
                }
            });

            // 映射 gRPC 服务
            endpoints.MapGrpcService<CatalogService>();

            // 配置健康检查终结点
            endpoints.MapHealthChecks("/hc", new HealthCheckOptions()
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
            endpoints.MapHealthChecks("/liveness", new HealthCheckOptions
            {
                Predicate = r => r.Name.Contains("self")
            });
        });

        // 配置事件总线
        ConfigureEventBus(app);
    }

    // 配置事件总线，订阅相关事件和处理程序
    protected virtual void ConfigureEventBus(IApplicationBuilder app)
    {
        var eventBus = app.ApplicationServices.GetRequiredService<IEventBus>();
        eventBus.Subscribe<OrderStatusChangedToAwaitingValidationIntegrationEvent, OrderStatusChangedToAwaitingValidationIntegrationEventHandler>();
        eventBus.Subscribe<OrderStatusChangedToPaidIntegrationEvent, OrderStatusChangedToPaidIntegrationEventHandler>();
    }
}

// 定义扩展方法，便于在 ConfigureServices 中注册各项服务
public static class CustomExtensionMethods
{
    // 添加 Application Insights 监控服务，并启用 Kubernetes 增强功能
    public static IServiceCollection AddAppInsight(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddApplicationInsightsTelemetry(configuration);
        services.AddApplicationInsightsKubernetesEnricher();

        return services;
    }

    // 添加自定义 MVC 配置，包括控制器、JSON 格式化和 CORS 策略
    public static IServiceCollection AddCustomMVC(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers(options =>
        {
            options.Filters.Add(typeof(HttpGlobalExceptionFilter));
        })
        .AddJsonOptions(options => options.JsonSerializerOptions.WriteIndented = true);

        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy",
                builder => builder
                .SetIsOriginAllowed((host) => true)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());
        });

        return services;
    }

    // 添加健康检查服务，包括 SQL Server、Azure Blob 存储和消息队列健康检查
    public static IServiceCollection AddCustomHealthCheck(this IServiceCollection services, IConfiguration configuration)
    {
        var accountName = configuration.GetValue<string>("AzureStorageAccountName");
        var accountKey = configuration.GetValue<string>("AzureStorageAccountKey");

        var hcBuilder = services.AddHealthChecks();

        hcBuilder
            .AddCheck("self", () => HealthCheckResult.Healthy())
            .AddSqlServer(
                configuration["ConnectionString"],
                name: "CatalogDB-check",
                tags: new string[] { "catalogdb" });

        if (!string.IsNullOrEmpty(accountName) && !string.IsNullOrEmpty(accountKey))
        {
            hcBuilder
                .AddAzureBlobStorage(
                    $"DefaultEndpointsProtocol=https;AccountName={accountName};AccountKey={accountKey};EndpointSuffix=core.windows.net",
                    name: "catalog-storage-check",
                    tags: new string[] { "catalogstorage" });
        }

        if (configuration.GetValue<bool>("AzureServiceBusEnabled"))
        {
            hcBuilder
                .AddAzureServiceBusTopic(
                    configuration["EventBusConnection"],
                    topicName: "eshop_event_bus",
                    name: "catalog-servicebus-check",
                    tags: new string[] { "servicebus" });
        }
        else
        {
            hcBuilder
                .AddRabbitMQ(
                    $"amqp://{configuration["EventBusConnection"]}",
                    name: "catalog-rabbitmqbus-check",
                    tags: new string[] { "rabbitmqbus" });
        }

        return services;
    }

    // 添加数据库上下文，配置 SQL Server 数据库和连接重试
    public static IServiceCollection AddCustomDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddEntityFrameworkSqlServer()
            .AddDbContext<CatalogContext>(options =>
        {
            options.UseSqlServer(configuration["ConnectionString"],
                                    sqlServerOptionsAction: sqlOptions =>
                                    {
                                        sqlOptions.MigrationsAssembly(typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
                                        // 配置 SQL Server 连接弹性重试策略
                                        sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                                    });
        });

        services.AddDbContext<IntegrationEventLogContext>(options =>
        {
            options.UseSqlServer(configuration["ConnectionString"],
                                    sqlServerOptionsAction: sqlOptions =>
                                    {
                                        sqlOptions.MigrationsAssembly(typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
                                        // 配置 SQL Server 连接弹性重试策略
                                        sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                                    });
        });

        return services;
    }

    // 添加自定义选项和 API 行为设置，如模型验证错误返回详细信息
    public static IServiceCollection AddCustomOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CatalogSettings>(configuration);
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var problemDetails = new ValidationProblemDetails(context.ModelState)
                {
                    Instance = context.HttpContext.Request.Path,
                    Status = StatusCodes.Status400BadRequest,
                    Detail = "Please refer to the errors property for additional details."
                };

                return new BadRequestObjectResult(problemDetails)
                {
                    ContentTypes = { "application/problem+json", "application/problem+xml" }
                };
            };
        });

        return services;
    }

    // 添加 Swagger 生成器，用于生成 API 文档
    public static IServiceCollection AddSwagger(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "eShopOnContainers - Catalog HTTP API",
                Version = "v1",
                Description = "The Catalog Microservice HTTP API. This is a Data-Driven/CRUD microservice sample"
            });
        });

        return services;
    }

    // 添加集成服务，注册事件日志服务和集成事件服务，同时根据配置添加不同的消息队列持久化连接
    public static IServiceCollection AddIntegrationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<Func<DbConnection, IIntegrationEventLogService>>(
            sp => (DbConnection c) => new IntegrationEventLogService(c));

        services.AddTransient<ICatalogIntegrationEventService, CatalogIntegrationEventService>();

        if (configuration.GetValue<bool>("AzureServiceBusEnabled"))
        {
            services.AddSingleton<IServiceBusPersisterConnection>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<CatalogSettings>>().Value;
                var serviceBusConnection = settings.EventBusConnection;

                return new DefaultServiceBusPersisterConnection(serviceBusConnection);
            });
        }
        else
        {
            services.AddSingleton<IRabbitMQPersistentConnection>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<CatalogSettings>>().Value;
                var logger = sp.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();

                var factory = new ConnectionFactory()
                {
                    HostName = configuration["EventBusConnection"],
                    DispatchConsumersAsync = true
                };

                if (!string.IsNullOrEmpty(configuration["EventBusUserName"]))
                {
                    factory.UserName = configuration["EventBusUserName"];
                }

                if (!string.IsNullOrEmpty(configuration["EventBusPassword"]))
                {
                    factory.Password = configuration["EventBusPassword"];
                }

                var retryCount = 5;
                if (!string.IsNullOrEmpty(configuration["EventBusRetryCount"]))
                {
                    retryCount = int.Parse(configuration["EventBusRetryCount"]);
                }

                return new DefaultRabbitMQPersistentConnection(factory, logger, retryCount);
            });
        }

        return services;
    }

    // 添加事件总线相关服务，根据配置添加 Azure Service Bus 或 RabbitMQ 的实现
    public static IServiceCollection AddEventBus(this IServiceCollection services, IConfiguration configuration)
    {
        if (configuration.GetValue<bool>("AzureServiceBusEnabled"))
        {
            services.AddSingleton<IEventBus, EventBusServiceBus>(sp =>
            {
                var serviceBusPersisterConnection = sp.GetRequiredService<IServiceBusPersisterConnection>();
                var iLifetimeScope = sp.GetRequiredService<ILifetimeScope>();
                var logger = sp.GetRequiredService<ILogger<EventBusServiceBus>>();
                var eventBusSubcriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();
                string subscriptionName = configuration["SubscriptionClientName"];

                return new EventBusServiceBus(serviceBusPersisterConnection, logger,
                    eventBusSubcriptionsManager, iLifetimeScope, subscriptionName);
            });
        }
        else
        {
            services.AddSingleton<IEventBus, EventBusRabbitMQ>(sp =>
            {
                var subscriptionClientName = configuration["SubscriptionClientName"];
                var rabbitMQPersistentConnection = sp.GetRequiredService<IRabbitMQPersistentConnection>();
                var iLifetimeScope = sp.GetRequiredService<ILifetimeScope>();
                var logger = sp.GetRequiredService<ILogger<EventBusRabbitMQ>>();
                var eventBusSubcriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();

                var retryCount = 5;
                if (!string.IsNullOrEmpty(configuration["EventBusRetryCount"]))
                {
                    retryCount = int.Parse(configuration["EventBusRetryCount"]);
                }

                return new EventBusRabbitMQ(rabbitMQPersistentConnection, logger, iLifetimeScope, eventBusSubcriptionsManager, subscriptionClientName, retryCount);
            });
        }

        services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();
        services.AddTransient<OrderStatusChangedToAwaitingValidationIntegrationEventHandler>();
        services.AddTransient<OrderStatusChangedToPaidIntegrationEventHandler>();

        return services;
    }
}
