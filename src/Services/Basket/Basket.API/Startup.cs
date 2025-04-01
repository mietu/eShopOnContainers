namespace Microsoft.eShopOnContainers.Services.Basket.API;
public class Startup
{
    // 构造函数，注入配置项
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    // 应用程序配置，用于读取appsettings.json及其他配置源
    public IConfiguration Configuration { get; }

    // 此方法在运行时调用，向容器注册服务
    public virtual IServiceProvider ConfigureServices(IServiceCollection services)
    {
        // 添加GRPC服务，启用详细错误信息，便于调试
        services.AddGrpc(options =>
        {
            options.EnableDetailedErrors = true;
        });

        // 注册Application Insights监控
        RegisterAppInsights(services);

        // 添加控制器服务，设置全局异常处理和模型验证过滤器
        services.AddControllers(options =>
            {
                options.Filters.Add(typeof(HttpGlobalExceptionFilter)); // 全局异步异常过滤器
                options.Filters.Add(typeof(ValidateModelStateFilter));   // 模型状态验证过滤器

            }) // 为功能测试添加程序集，设置JSON格式化缩进输出
            .AddApplicationPart(typeof(BasketController).Assembly)
            .AddJsonOptions(options => options.JsonSerializerOptions.WriteIndented = true);

        // 添加Swagger生成器，用于生成HTTP API文档
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "eShopOnContainers - Basket HTTP API",
                Version = "v1",
                Description = "The Basket Service HTTP API"
            });

            // 定义OAuth2安全方案
            options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows()
                {
                    Implicit = new OpenApiOAuthFlow()
                    {
                        AuthorizationUrl = new Uri($"{Configuration.GetValue<string>("IdentityUrlExternal")}/connect/authorize"),
                        TokenUrl = new Uri($"{Configuration.GetValue<string>("IdentityUrlExternal")}/connect/token"),
                        Scopes = new Dictionary<string, string>()
                        {
                                { "basket", "Basket API" }
                        }
                    }
                }
            });

            // 操作过滤器，添加授权检查
            options.OperationFilter<AuthorizeCheckOperationFilter>();
        });

        // 配置身份验证服务及授权策略
        ConfigureAuthService(services);

        // 添加健康检查服务，用于监控应用运行状态
        services.AddCustomHealthCheck(Configuration);

        // 将BasketSettings配置绑定到配置文件
        services.Configure<BasketSettings>(Configuration);

        // 注册Redis连接，确保在服务启动前Redis连接已经建立
        services.AddSingleton<ConnectionMultiplexer>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<BasketSettings>>().Value;
            var configuration = ConfigurationOptions.Parse(settings.ConnectionString, true);

            return ConnectionMultiplexer.Connect(configuration);
        });

        // 根据配置判断是否使用Azure Service Bus
        if (Configuration.GetValue<bool>("AzureServiceBusEnabled"))
        {
            // 如果启用Azure Service Bus，注册对应的Persister Connection
            services.AddSingleton<IServiceBusPersisterConnection>(sp =>
            {
                var serviceBusConnectionString = Configuration["EventBusConnection"];
                return new DefaultServiceBusPersisterConnection(serviceBusConnectionString);
            });
        }
        else
        {
            // 如果未启用Azure Service Bus，则使用RabbitMQ连接
            services.AddSingleton<IRabbitMQPersistentConnection>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();

                var factory = new ConnectionFactory()
                {
                    HostName = Configuration["EventBusConnection"],
                    DispatchConsumersAsync = true
                };

                if (!string.IsNullOrEmpty(Configuration["EventBusUserName"]))
                {
                    factory.UserName = Configuration["EventBusUserName"];
                }

                if (!string.IsNullOrEmpty(Configuration["EventBusPassword"]))
                {
                    factory.Password = Configuration["EventBusPassword"];
                }

                var retryCount = 5;
                if (!string.IsNullOrEmpty(Configuration["EventBusRetryCount"]))
                {
                    retryCount = int.Parse(Configuration["EventBusRetryCount"]);
                }

                return new DefaultRabbitMQPersistentConnection(factory, logger, retryCount);
            });
        }

        // 注册事件总线及其事件处理程序
        RegisterEventBus(services);

        // 配置跨域策略，允许任意来源访问
        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy",
                builder => builder
                .SetIsOriginAllowed((host) => true)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());
        });

        // 注册HttpContextAccessor，方便在其他服务中访问HttpContext
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        // 注入篮子仓储和身份验证服务
        services.AddTransient<IBasketRepository, RedisBasketRepository>();
        services.AddTransient<IIdentityService, IdentityService>();

        // 添加Options模式支持
        services.AddOptions();

        // 使用Autofac作为依赖注入容器，并填充已注册的服务
        var container = new ContainerBuilder();
        container.Populate(services);

        return new AutofacServiceProvider(container.Build());
    }

    // 此方法在运行时调用，构建HTTP请求管线
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
    {
        // 可选：添加Azure Web应用诊断日志
        //loggerFactory.AddAzureWebAppDiagnostics();
        //loggerFactory.AddApplicationInsights(app.ApplicationServices, LogLevel.Trace);

        // 判断是否设置了PATH_BASE，如果设置，则应用该基路径
        var pathBase = Configuration["PATH_BASE"];
        if (!string.IsNullOrEmpty(pathBase))
        {
            app.UsePathBase(pathBase);
        }

        // 配置Swagger中间件，用于生成和展示API文档
        app.UseSwagger()
            .UseSwaggerUI(setup =>
            {
                setup.SwaggerEndpoint($"{(!string.IsNullOrEmpty(pathBase) ? pathBase : string.Empty)}/swagger/v1/swagger.json", "Basket.API V1");
                setup.OAuthClientId("basketswaggerui");
                setup.OAuthAppName("Basket Swagger UI");
            });

        // 启用路由和跨域中间件
        app.UseRouting();
        app.UseCors("CorsPolicy");

        // 启用身份验证和授权
        ConfigureAuth(app);

        // 启用静态文件服务
        app.UseStaticFiles();

        // 配置终结点路由
        app.UseEndpoints(endpoints =>
        {
            // 注册GRPC服务
            endpoints.MapGrpcService<BasketService>();

            // 注册默认控制器路由以及控制器端点
            endpoints.MapDefaultControllerRoute();
            endpoints.MapControllers();

            // 映射获取Proto文件的端点，返回Proto定义文本
            endpoints.MapGet("/_proto/", async ctx =>
            {
                ctx.Response.ContentType = "text/plain";
                using var fs = new FileStream(Path.Combine(env.ContentRootPath, "Proto", "basket.proto"), FileMode.Open, FileAccess.Read);
                using var sr = new StreamReader(fs);
                while (!sr.EndOfStream)
                {
                    var line = await sr.ReadLineAsync();
                    // 跳过指定的注释标识符行
                    if (line != "/* >>" || line != "<< */")
                    {
                        await ctx.Response.WriteAsync(line);
                    }
                }
            });

            // 配置健康检查端点
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

        // 配置事件总线，订阅相关事件
        ConfigureEventBus(app);
    }

    // 注册Application Insights相关服务
    private void RegisterAppInsights(IServiceCollection services)
    {
        services.AddApplicationInsightsTelemetry(Configuration);
        services.AddApplicationInsightsKubernetesEnricher();
    }

    // 设置身份验证和授权服务，使用JWT Bearer令牌
    private void ConfigureAuthService(IServiceCollection services)
    {
        // 防止系统默认将"sub"映射到NameIdentifier
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("sub");

        var identityUrl = Configuration.GetValue<string>("IdentityUrl");

        services.AddAuthentication("Bearer").AddJwtBearer(options =>
        {
            options.Authority = identityUrl;
            options.RequireHttpsMetadata = false;
            options.Audience = "basket";
            options.TokenValidationParameters.ValidateAudience = false;
        });
        services.AddAuthorization(options =>
        {
            options.AddPolicy("ApiScope", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("scope", "basket");
            });
        });
    }

    // 配置中间件使用认证和授权
    protected virtual void ConfigureAuth(IApplicationBuilder app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
    }

    // 注册事件总线组件以及事件处理器
    private void RegisterEventBus(IServiceCollection services)
    {
        if (Configuration.GetValue<bool>("AzureServiceBusEnabled"))
        {
            // 如果启用Azure Service Bus，注册对应的事件总线实现
            services.AddSingleton<IEventBus, EventBusServiceBus>(sp =>
            {
                var serviceBusPersisterConnection = sp.GetRequiredService<IServiceBusPersisterConnection>();
                var iLifetimeScope = sp.GetRequiredService<ILifetimeScope>();
                var logger = sp.GetRequiredService<ILogger<EventBusServiceBus>>();
                var eventBusSubscriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();
                string subscriptionName = Configuration["SubscriptionClientName"];

                return new EventBusServiceBus(serviceBusPersisterConnection, logger,
                    eventBusSubscriptionsManager, iLifetimeScope, subscriptionName);
            });
        }
        else
        {
            // 使用RabbitMQ实现的事件总线
            services.AddSingleton<IEventBus, EventBusRabbitMQ>(sp =>
            {
                var subscriptionClientName = Configuration["SubscriptionClientName"];
                var rabbitMQPersistentConnection = sp.GetRequiredService<IRabbitMQPersistentConnection>();
                var iLifetimeScope = sp.GetRequiredService<ILifetimeScope>();
                var logger = sp.GetRequiredService<ILogger<EventBusRabbitMQ>>();
                var eventBusSubscriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();

                var retryCount = 5;
                if (!string.IsNullOrEmpty(Configuration["EventBusRetryCount"]))
                {
                    retryCount = int.Parse(Configuration["EventBusRetryCount"]);
                }

                return new EventBusRabbitMQ(rabbitMQPersistentConnection, logger, iLifetimeScope, eventBusSubscriptionsManager, subscriptionClientName, retryCount);
            });
        }

        // 注册内存中事件订阅管理器，用于管理事件处理的订阅
        services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();

        // 注入具体的事件处理器
        services.AddTransient<ProductPriceChangedIntegrationEventHandler>();
        services.AddTransient<OrderStartedIntegrationEventHandler>();
    }

    // 将事件订阅到相应的处理器中
    private void ConfigureEventBus(IApplicationBuilder app)
    {
        var eventBus = app.ApplicationServices.GetRequiredService<IEventBus>();

        eventBus.Subscribe<ProductPriceChangedIntegrationEvent, ProductPriceChangedIntegrationEventHandler>();
        eventBus.Subscribe<OrderStartedIntegrationEvent, OrderStartedIntegrationEventHandler>();
    }
}
