namespace Microsoft.eShopOnContainers.Services.Ordering.API;

// Startup类负责配置应用程序的服务和中间件管道
public class Startup
{
    // 构造函数注入配置对象
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration; // 初始化配置
    }

    // 保存配置信息
    public IConfiguration Configuration { get; }

    // 配置服务容器（依赖注入）
    // 在这里注册各种服务，例如gRPC、MVC、健康检查、数据库、Swagger、认证、事件总线等
    public virtual IServiceProvider ConfigureServices(IServiceCollection services)
    {
        services
            .AddGrpc(options =>
            {
                options.EnableDetailedErrors = true; // 启用详细错误信息，便于调试gRPC服务
            })
            .Services
            // 添加Application Insights监控
            .AddApplicationInsights(Configuration)
            // 添加自定义的MVC配置
            .AddCustomMvc()
            // 添加健康检查服务
            .AddHealthChecks(Configuration)
            // 添加自定义数据库上下文注册
            .AddCustomDbContext(Configuration)
            // 添加Swagger生成API文档
            .AddCustomSwagger(Configuration)
            // 添加JWT认证
            .AddCustomAuthentication(Configuration)
            // 添加授权策略
            .AddCustomAuthorization(Configuration)
            // 添加各种集成服务，例如集成事件处理
            .AddCustomIntegrations(Configuration)
            // 添加自定义配置选项
            .AddCustomConfiguration(Configuration)
            // 添加事件总线服务，用于发布和订阅集成事件
            .AddEventBus(Configuration);

        // 配置Autofac作为依赖注入容器替代默认DI容器
        var container = new ContainerBuilder();
        // 将已注册的服务填充到Autofac容器中
        container.Populate(services);

        // 注册中介模块（Mediator模式的实现）
        container.RegisterModule(new MediatorModule());
        // 注册应用程序模块，传入数据库连接字符串
        container.RegisterModule(new ApplicationModule(Configuration["ConnectionString"]));

        // 构建容器，并返回Autofac服务提供者
        return new AutofacServiceProvider(container.Build());
    }

    // 配置应用程序的中间件请求管道
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
    {
        // 可选：配置Azure Web App和Application Insights日志记录（已注释）
        //loggerFactory.AddAzureWebAppDiagnostics();
        //loggerFactory.AddApplicationInsights(app.ApplicationServices, LogLevel.Trace);

        // 根据配置设置应用程序的基础路径
        var pathBase = Configuration["PATH_BASE"];
        if (!string.IsNullOrEmpty(pathBase))
        {
            loggerFactory.CreateLogger<Startup>().LogDebug("Using PATH BASE '{pathBase}'", pathBase);
            app.UsePathBase(pathBase); // 使用指定的路径基
        }

        // 启用Swagger生成接口文档及Swagger UI
        app.UseSwagger()
            .UseSwaggerUI(c =>
            {
                // 配置Swagger JSON端点，支持基础路径
                c.SwaggerEndpoint($"{(!string.IsNullOrEmpty(pathBase) ? pathBase : string.Empty)}/swagger/v1/swagger.json", "Ordering.API V1");
                c.OAuthClientId("orderingswaggerui"); // 配置OAuth客户端ID
                c.OAuthAppName("Ordering Swagger UI"); // 配置应用名称
            });

        app.UseRouting(); // 启用路由中间件
        app.UseCors("CorsPolicy"); // 使用CORS策略，允许跨域访问

        // 调用配置认证方法
        ConfigureAuth(app);

        // 配置终结点映射
        app.UseEndpoints(endpoints =>
        {
            // 映射gRPC服务
            endpoints.MapGrpcService<OrderingService>();
            // 映射默认控制器路由（MVC）
            endpoints.MapDefaultControllerRoute();
            // 映射所有控制器
            endpoints.MapControllers();
            // 映射/_proto/路由，用于读取并返回proto文件内容
            endpoints.MapGet("/_proto/", async ctx =>
            {
                ctx.Response.ContentType = "text/plain";
                // 从指定路径读取文件
                using var fs = new FileStream(Path.Combine(env.ContentRootPath, "Proto", "basket.proto"), FileMode.Open, FileAccess.Read);
                using var sr = new StreamReader(fs);
                // 逐行读取文件内容，并写入响应中
                while (!sr.EndOfStream)
                {
                    var line = await sr.ReadLineAsync();
                    // 忽略特定标记行
                    if (line != "/* >>" || line != "<< */")
                    {
                        await ctx.Response.WriteAsync(line);
                    }
                }
            });
            // 映射健康检查端点（/hc）并使用UI响应编写器返回检查结果
            endpoints.MapHealthChecks("/hc", new HealthCheckOptions()
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
            // 映射存活性检查端点（/liveness），仅检查名称中包含"self"的健康检查项
            endpoints.MapHealthChecks("/liveness", new HealthCheckOptions
            {
                Predicate = r => r.Name.Contains("self")
            });
        });

        // 配置事件总线，用于消息订阅
        ConfigureEventBus(app);
    }

    // 配置事件总线订阅，订阅一系列集成事件以触发相应处理器
    private void ConfigureEventBus(IApplicationBuilder app)
    {
        // 从服务容器中获取事件总线实例
        var eventBus = app.ApplicationServices.GetRequiredService<BuildingBlocks.EventBus.Abstractions.IEventBus>();

        // 订阅各种业务领域中可能发生的集成事件，并指定对应的事件处理器接口
        eventBus.Subscribe<UserCheckoutAcceptedIntegrationEvent, IIntegrationEventHandler<UserCheckoutAcceptedIntegrationEvent>>();
        eventBus.Subscribe<GracePeriodConfirmedIntegrationEvent, IIntegrationEventHandler<GracePeriodConfirmedIntegrationEvent>>();
        eventBus.Subscribe<OrderStockConfirmedIntegrationEvent, IIntegrationEventHandler<OrderStockConfirmedIntegrationEvent>>();
        eventBus.Subscribe<OrderStockRejectedIntegrationEvent, IIntegrationEventHandler<OrderStockRejectedIntegrationEvent>>();
        eventBus.Subscribe<OrderPaymentFailedIntegrationEvent, IIntegrationEventHandler<OrderPaymentFailedIntegrationEvent>>();
        eventBus.Subscribe<OrderPaymentSucceededIntegrationEvent, IIntegrationEventHandler<OrderPaymentSucceededIntegrationEvent>>();
    }

    // 配置认证和授权中间件
    protected virtual void ConfigureAuth(IApplicationBuilder app)
    {
        app.UseAuthentication(); // 启用认证（验证用户身份）
        app.UseAuthorization();  // 启用授权（检查用户权限）
    }
}

// 定义一系列的扩展方法，用于扩展IServiceCollection以注册常用服务
static class CustomExtensionsMethods
{
    // 添加Application Insights，监控和日志记录
    public static IServiceCollection AddApplicationInsights(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddApplicationInsightsTelemetry(configuration); // 启用Application Insights遥测
        services.AddApplicationInsightsKubernetesEnricher();        // 添加Kubernetes环境的增强信息

        return services;
    }

    // 添加自定义MVC服务，包括控制器、CORS和JSON配置
    public static IServiceCollection AddCustomMvc(this IServiceCollection services)
    {
        // 添加控制器，同时注册全局异常过滤器，便于统一处理异常
        services.AddControllers(options =>
            {
                options.Filters.Add(typeof(HttpGlobalExceptionFilter)); // 添加全局异常过滤器
            })
            // 为功能测试添加扩展，注册包含OrdersController的应用程序集
            .AddApplicationPart(typeof(OrdersController).Assembly)
            .AddJsonOptions(options => options.JsonSerializerOptions.WriteIndented = true); // 配置JSON缩进格式

        // 配置跨域访问策略
        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy",
                builder => builder
                .SetIsOriginAllowed((host) => true)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()); // 允许任何来源、方法和页头，并允许凭据传输
        });

        return services;
    }

    // 添加健康检查服务，检查各个依赖项的状态
    public static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var hcBuilder = services.AddHealthChecks();

        // 添加自我健康检查
        hcBuilder.AddCheck("self", () => HealthCheckResult.Healthy());

        // 添加SQL Server健康检查
        hcBuilder
            .AddSqlServer(
                configuration["ConnectionString"],
                name: "OrderingDB-check",
                tags: new string[] { "orderingdb" });

        // 根据配置添加Azure Service Bus或RabbitMQ的健康检查
        if (configuration.GetValue<bool>("AzureServiceBusEnabled"))
        {
            hcBuilder
                .AddAzureServiceBusTopic(
                    configuration["EventBusConnection"],
                    topicName: "eshop_event_bus",
                    name: "ordering-servicebus-check",
                    tags: new string[] { "servicebus" });
        }
        else
        {
            hcBuilder
                .AddRabbitMQ(
                    $"amqp://{configuration["EventBusConnection"]}",
                    name: "ordering-rabbitmqbus-check",
                    tags: new string[] { "rabbitmqbus" });
        }

        return services;
    }

    // 注册自定义数据库上下文，包括OrderingContext和IntegrationEventLogContext，
    // 配置连接字符串、迁移程序集和重试策略
    public static IServiceCollection AddCustomDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<OrderingContext>(options =>
                {
                    options.UseSqlServer(configuration["ConnectionString"],
                        sqlServerOptionsAction: sqlOptions =>
                        {
                            sqlOptions.MigrationsAssembly(typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
                            // 配置连接重试策略：最多15次重试，最大延迟30秒
                            sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                        });
                },
                    ServiceLifetime.Scoped); // DbContext在HTTP请求内共享

        services.AddDbContext<IntegrationEventLogContext>(options =>
        {
            options.UseSqlServer(configuration["ConnectionString"],
                                    sqlServerOptionsAction: sqlOptions =>
                                    {
                                        sqlOptions.MigrationsAssembly(typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
                                        sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                                    });
        });

        return services;
    }

    // 配置Swagger，生成OpenApi文档, 并定义安全认证方案
    public static IServiceCollection AddCustomSwagger(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSwaggerGen(options =>
        {
            // 定义Swagger文档信息
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "eShopOnContainers - Ordering HTTP API",
                Version = "v1",
                Description = "The Ordering Service HTTP API"
            });
            // 定义OAuth2安全性方案
            options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows()
                {
                    Implicit = new OpenApiOAuthFlow()
                    {
                        AuthorizationUrl = new Uri($"{configuration.GetValue<string>("IdentityUrlExternal")}/connect/authorize"),
                        TokenUrl = new Uri($"{configuration.GetValue<string>("IdentityUrlExternal")}/connect/token"),
                        Scopes = new Dictionary<string, string>()
                        {
                                { "orders", "Ordering API" }
                        }
                    }
                }
            });

            // 添加操作过滤器，自动添加授权检查
            options.OperationFilter<AuthorizeCheckOperationFilter>();
        });

        return services;
    }

    // 添加集成服务，包括HttpContext访问器、身份验证服务和集成事件日志服务
    public static IServiceCollection AddCustomIntegrations(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>(); // 添加HttpContext访问器
        services.AddTransient<IIdentityService, IdentityService>(); // 添加身份验证服务
        services.AddTransient<Func<DbConnection, IIntegrationEventLogService>>(
            sp => (DbConnection c) => new IntegrationEventLogService(c)); // 添加集成事件日志服务

        services.AddTransient<IOrderingIntegrationEventService, OrderingIntegrationEventService>(); // 添加订单集成事件服务

        // 根据配置添加Azure Service Bus或RabbitMQ持久连接
        if (configuration.GetValue<bool>("AzureServiceBusEnabled"))
        {
            services.AddSingleton<IServiceBusPersisterConnection>(sp =>
            {
                var serviceBusConnectionString = configuration["EventBusConnection"];
                var subscriptionClientName = configuration["SubscriptionClientName"];
                return new DefaultServiceBusPersisterConnection(serviceBusConnectionString);
            });
        }
        else
        {
            services.AddSingleton<IRabbitMQPersistentConnection>(sp =>
            {
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

    // 添加自定义配置，用于注册选项和全局API行为配置
    public static IServiceCollection AddCustomConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions();
        services.Configure<OrderingSettings>(configuration); // 配置订单设置
                                                             // 配置API行为, 当模型验证失败时返回自定义错误信息
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

    // 添加事件总线服务，根据Azure Service Bus开关选择不同实现
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

        // 添加内存事件总线订阅管理器（管理事件订阅关系）
        services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();

        return services;
    }

    // 添加JWT认证，防止将"sub"映射到nameidentifier，并配置授权服务器地址和受众
    public static IServiceCollection AddCustomAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        // 防止将"sub"声明映射到nameidentifier
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("sub");

        var identityUrl = configuration.GetValue<string>("IdentityUrl");

        services.AddAuthentication("Bearer").AddJwtBearer(options =>
        {
            options.Authority = identityUrl;
            options.RequireHttpsMetadata = false;
            options.Audience = "orders";
            options.TokenValidationParameters.ValidateAudience = false;
        });

        return services;
    }

    // 添加授权策略，指定必须认证并拥有对应的scope声明
    public static IServiceCollection AddCustomAuthorization(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("ApiScope", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("scope", "orders");
            });
        });
        return services;
    }
}
