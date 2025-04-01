namespace Microsoft.eShopOnContainers.Mobile.Shopping.HttpAggregator;

// 启动类，用于配置应用程序服务和HTTP管道
public class Startup
{
    // 通过依赖注入获取配置对象
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // 该方法在运行时被调用，用于注册服务到依赖注入容器
    public void ConfigureServices(IServiceCollection services)
    {
        // 添加健康检查服务，并注册了多个依赖于外部API健康URL检查
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy()) // 自检健康状况
            .AddUrlGroup(new Uri(Configuration["CatalogUrlHC"]), name: "catalogapi-check", tags: new string[] { "catalogapi" })
            .AddUrlGroup(new Uri(Configuration["OrderingUrlHC"]), name: "orderingapi-check", tags: new string[] { "orderingapi" })
            .AddUrlGroup(new Uri(Configuration["BasketUrlHC"]), name: "basketapi-check", tags: new string[] { "basketapi" })
            .AddUrlGroup(new Uri(Configuration["IdentityUrlHC"]), name: "identityapi-check", tags: new string[] { "identityapi" })
            .AddUrlGroup(new Uri(Configuration["PaymentUrlHC"]), name: "paymentapi-check", tags: new string[] { "paymentapi" });

        // 调用扩展方法添加自定义MVC、认证、开发沙箱支持、HTTP和gRPC服务
        services.AddCustomMvc(Configuration)
                .AddCustomAuthentication(Configuration)
                .AddDevspaces()
                .AddHttpServices()
                .AddGrpcServices();
    }

    // 该方法在运行时被调用，用于配置HTTP请求管道
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
    {
        // 获取配置中的PATH_BASE，允许应用部署在子路径下
        var pathBase = Configuration["PATH_BASE"];

        if (!string.IsNullOrEmpty(pathBase))
        {
            // 记录路径基调试信息
            loggerFactory.CreateLogger<Startup>().LogDebug("Using PATH BASE '{pathBase}'", pathBase);
            // 设置请求路径前缀
            app.UsePathBase(pathBase);
        }

        if (env.IsDevelopment())
        {
            // 开发环境下显示详细错误信息页面
            app.UseDeveloperExceptionPage();
        }

        // 配置Swagger中间件，用于生成API文档
        app.UseSwagger().UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint($"{(!string.IsNullOrEmpty(pathBase) ? pathBase : string.Empty)}/swagger/v1/swagger.json", "Purchase BFF V1");

            // 设置Swagger认证相关配置
            c.OAuthClientId("mobileshoppingaggswaggerui");
            c.OAuthClientSecret(string.Empty);
            c.OAuthRealm(string.Empty);
            c.OAuthAppName("Purchase BFF Swagger UI");
        });

        // 启用路由中间件
        app.UseRouting();
        // 启用跨域策略
        app.UseCors("CorsPolicy");
        // 启用认证与授权中间件
        app.UseAuthentication();
        app.UseAuthorization();
        // 配置终结点映射
        app.UseEndpoints(endpoints =>
        {
            // 映射默认控制器路由
            endpoints.MapDefaultControllerRoute();
            // 映射所有控制器
            endpoints.MapControllers();
            // 配置健康检查端点，返回健康状态
            endpoints.MapHealthChecks("/hc", new HealthCheckOptions()
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
            // 配置存活检查端点，仅包含 "self" 检查
            endpoints.MapHealthChecks("/liveness", new HealthCheckOptions
            {
                Predicate = r => r.Name.Contains("self")
            });
        });
    }
}

// 扩展方法集合，用于向 IServiceCollection 添加自定义服务
public static class ServiceCollectionExtensions
{
    // 添加自定义MVC服务，包括控制器、JSON格式设置、以及Swagger文档生成
    public static IServiceCollection AddCustomMvc(this IServiceCollection services, IConfiguration configuration)
    {
        // 添加配置支持
        services.AddOptions();
        // 绑定配置文件中的"urls"部分到UrlsConfig类
        services.Configure<UrlsConfig>(configuration.GetSection("urls"));

        services.AddControllers()
                .AddJsonOptions(options => options.JsonSerializerOptions.WriteIndented = true);

        // 配置Swagger生成器以生成API文档
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Shopping Aggregator for Mobile Clients",
                Version = "v1",
                Description = "Shopping Aggregator for Mobile Clients"
            });
            // 添加安全定义，用于OAuth2认证
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
                            { "mobileshoppingagg", "Shopping Aggregator for Mobile Clients" }
                        }
                    }
                }
            });

            options.OperationFilter<AuthorizeCheckOperationFilter>();
        });

        // 配置跨域访问策略
        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy",
                builder => builder
                .AllowAnyMethod()
                .AllowAnyHeader()
                .SetIsOriginAllowed((host) => true)
                .AllowCredentials());
        });

        return services;
    }

    // 添加自定义认证服务，主要使用JwtBearer方案
    public static IServiceCollection AddCustomAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        // 删除默认映射的"sub"声明，避免与JwtBearer认证冲突
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("sub");

        // 从配置文件中获取identity服务的URL
        var identityUrl = configuration.GetValue<string>("urls:identity");

        // 配置默认认证方案为JwtBearer
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.Authority = identityUrl;
            options.RequireHttpsMetadata = false;
            options.Audience = "mobileshoppingagg";
            // 配置令牌验证参数，不验证Audience
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false
            };
        });

        return services;
    }

    // 添加自定义授权服务，配置基于ApiScope的策略
    public static IServiceCollection AddCustomAuthorization(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("ApiScope", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("scope", "mobileshoppingagg");
            });
        });
        return services;
    }

    // 添加HTTP相关服务，包括HTTP客户端配置和相关委托处理器
    public static IServiceCollection AddHttpServices(this IServiceCollection services)
    {
        // 注册HTTP客户端授权处理器
        services.AddTransient<HttpClientAuthorizationDelegatingHandler>();
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        // 注册订单API客户端，并支持开发沙箱配置
        services.AddHttpClient<IOrderApiClient, OrderApiClient>()
                .AddDevspacesSupport();

        return services;
    }

    // 添加gRPC相关服务，包括注册客户端和异常拦截器
    public static IServiceCollection AddGrpcServices(this IServiceCollection services)
    {
        // 注册gRPC异常拦截器
        services.AddTransient<GrpcExceptionInterceptor>();

        // 注册购物车服务
        services.AddScoped<IBasketService, BasketService>();

        // 注册并配置gRPC客户端，用于访问Basket API
        services.AddGrpcClient<Basket.BasketClient>((services, options) =>
        {
            var basketApi = services.GetRequiredService<IOptions<UrlsConfig>>().Value.GrpcBasket;
            options.Address = new Uri(basketApi);
        }).AddInterceptor<GrpcExceptionInterceptor>();

        // 注册目录服务
        services.AddScoped<ICatalogService, CatalogService>();

        // 注册并配置gRPC客户端，用于访问Catalog API
        services.AddGrpcClient<Catalog.CatalogClient>((services, options) =>
        {
            var catalogApi = services.GetRequiredService<IOptions<UrlsConfig>>().Value.GrpcCatalog;
            options.Address = new Uri(catalogApi);
        }).AddInterceptor<GrpcExceptionInterceptor>();

        // 注册订单服务
        services.AddScoped<IOrderingService, OrderingService>();

        // 注册并配置gRPC客户端，用于访问Ordering API
        services.AddGrpcClient<OrderingGrpc.OrderingGrpcClient>((services, options) =>
        {
            var orderingApi = services.GetRequiredService<IOptions<UrlsConfig>>().Value.GrpcOrdering;
            options.Address = new Uri(orderingApi);
        }).AddInterceptor<GrpcExceptionInterceptor>();

        return services;
    }
}
