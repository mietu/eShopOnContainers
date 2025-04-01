namespace Microsoft.eShopOnContainers.Web.Shopping.HttpAggregator;

// Startup 类用于配置应用程序的服务和请求管道
public class Startup
{
    // 构造函数，通过 IConfiguration 实例加载配置
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // 当运行时调用此方法时，用于向 DI 容器中添加服务
    public void ConfigureServices(IServiceCollection services)
    {
        // 添加健康检查服务，并为各个后端 API 添加健康检查
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy()) // 自身健康检查
            .AddUrlGroup(new Uri(Configuration["CatalogUrlHC"]), name: "catalogapi-check", tags: new string[] { "catalogapi" })
            .AddUrlGroup(new Uri(Configuration["OrderingUrlHC"]), name: "orderingapi-check", tags: new string[] { "orderingapi" })
            .AddUrlGroup(new Uri(Configuration["BasketUrlHC"]), name: "basketapi-check", tags: new string[] { "basketapi" })
            .AddUrlGroup(new Uri(Configuration["IdentityUrlHC"]), name: "identityapi-check", tags: new string[] { "identityapi" })
            .AddUrlGroup(new Uri(Configuration["PaymentUrlHC"]), name: "paymentapi-check", tags: new string[] { "paymentapi" });

        // 配置 MVC、认证相关、中间件服务以及应用层、gRPC 服务等
        services.AddCustomMvc(Configuration)
            .AddCustomAuthentication(Configuration)
            //.AddCustomAuthorization(Configuration)
            .AddDevspaces()
            .AddApplicationServices()
            .AddGrpcServices();
    }

    // 该方法配置 HTTP 请求管道
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
    {
        // 如果配置中存在 PATH_BASE，则设置应用程序的路径基础
        var pathBase = Configuration["PATH_BASE"];
        if (!string.IsNullOrEmpty(pathBase))
        {
            loggerFactory.CreateLogger<Startup>().LogDebug("Using PATH BASE '{pathBase}'", pathBase);
            app.UsePathBase(pathBase);
        }

        // 开发环境下使用开发者异常页面
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        // 强制 HTTPS
        app.UseHttpsRedirection();

        // 配置 Swagger 生成和 UI
        app.UseSwagger().UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint($"{(!string.IsNullOrEmpty(pathBase) ? pathBase : string.Empty)}/swagger/v1/swagger.json", "Purchase BFF V1");

            c.OAuthClientId("webshoppingaggswaggerui");
            c.OAuthClientSecret(string.Empty);
            c.OAuthRealm(string.Empty);
            c.OAuthAppName("web shopping bff Swagger UI");
        });

        // 启用路由
        app.UseRouting();
        // 启用跨域策略
        app.UseCors("CorsPolicy");
        // 启用认证和授权中间件
        app.UseAuthentication();
        app.UseAuthorization();

        // 配置终结点映射
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapDefaultControllerRoute(); // 映射默认路由
            endpoints.MapControllers(); // 映射控制器路由

            // 映射健康检查终结点 "/hc"，返回详细健康检查信息
            endpoints.MapHealthChecks("/hc", new HealthCheckOptions()
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
            // 映射存活性检查终结点 "/liveness"
            endpoints.MapHealthChecks("/liveness", new HealthCheckOptions
            {
                Predicate = r => r.Name.Contains("self")
            });
        });
    }
}

// 扩展方法类，用于注册自定义服务
public static class ServiceCollectionExtensions
{
    // 添加自定义认证服务，使用 JWT Bearer 认证方案
    public static IServiceCollection AddCustomAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        // 移除默认的 "sub" 声明映射，使 JWT 中的 "sub" 原样传递
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("sub");

        var identityUrl = configuration.GetValue<string>("urls:identity");
        services.AddAuthentication("Bearer")
        .AddJwtBearer(options =>
        {
            options.Authority = identityUrl; // 颁发机构 URL
            options.RequireHttpsMetadata = false; // 测试模式下允许 HTTP
            options.Audience = "webshoppingagg";  // 目标受众
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false // 可以关闭受众验证
            };
        });

        return services;
    }

    // 添加自定义 MVC 服务和 Swagger 配置
    public static IServiceCollection AddCustomMvc(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions();
        // 将 urls 配置部分映射到 UrlsConfig 配置类
        services.Configure<UrlsConfig>(configuration.GetSection("urls"));

        // 添加控制器并设置 JSON 输出格式
        services.AddControllers()
                .AddJsonOptions(options => options.JsonSerializerOptions.WriteIndented = true);

        // 添加 Swagger 生成器，方便 API 文档生成
        services.AddSwaggerGen(options =>
        {
            // 定义一个 Swagger 文档
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Shopping Aggregator for Web Clients",
                Version = "v1",
                Description = "Shopping Aggregator for Web Clients"
            });

            // 添加 OAuth2 安全定义配置
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
                            { "webshoppingagg", "Shopping Aggregator for Web Clients" }
                        }
                    }
                }
            });

            // 使用操作过滤器添加认证检查
            options.OperationFilter<AuthorizeCheckOperationFilter>();
        });

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

        return services;
    }

    // 添加应用程序级服务，如 HttpClient 和 Delegating Handler 等
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // 注册 HttpClient 授权处理程序（Delegating Handler）
        services.AddTransient<HttpClientAuthorizationDelegatingHandler>();
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        // 注册 HttpClient，用于访问订单 API，同时添加授权处理程序
        services.AddHttpClient<IOrderApiClient, OrderApiClient>()
            .AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>()
            .AddDevspacesSupport();

        return services;
    }

    // 添加 gRPC 相关服务
    public static IServiceCollection AddGrpcServices(this IServiceCollection services)
    {
        // 注册 gRPC 拦截器（用于处理异常）
        services.AddTransient<GrpcExceptionInterceptor>();

        // 注册 Basket 服务并配置 gRPC 客户端
        services.AddScoped<IBasketService, BasketService>();

        services.AddGrpcClient<Basket.BasketClient>((services, options) =>
        {
            var basketApi = services.GetRequiredService<IOptions<UrlsConfig>>().Value.GrpcBasket;
            options.Address = new Uri(basketApi);
        }).AddInterceptor<GrpcExceptionInterceptor>();

        // 注册 Catalog 服务并配置 gRPC 客户端
        services.AddScoped<ICatalogService, CatalogService>();

        services.AddGrpcClient<Catalog.CatalogClient>((services, options) =>
        {
            var catalogApi = services.GetRequiredService<IOptions<UrlsConfig>>().Value.GrpcCatalog;
            options.Address = new Uri(catalogApi);
        }).AddInterceptor<GrpcExceptionInterceptor>();

        // 注册 Ordering 服务并配置 gRPC 客户端
        services.AddScoped<IOrderingService, OrderingService>();

        services.AddGrpcClient<OrderingGrpc.OrderingGrpcClient>((services, options) =>
        {
            var orderingApi = services.GetRequiredService<IOptions<UrlsConfig>>().Value.GrpcOrdering;
            options.Address = new Uri(orderingApi);
        }).AddInterceptor<GrpcExceptionInterceptor>();

        return services;
    }
}
