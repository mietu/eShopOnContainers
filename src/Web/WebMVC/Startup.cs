namespace Microsoft.eShopOnContainers.WebMVC;

/// <summary>
/// 应用程序启动类，负责配置服务和HTTP请求处理管道
/// </summary>
public class Startup
{
    /// <summary>
    /// 构造函数，接收应用程序配置
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
    /// 配置应用程序的服务
    /// 此方法由运行时调用，用于向IoC容器添加服务
    /// </summary>
    /// <param name="services">服务集合</param>
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllersWithViews()  // 添加MVC服务
            .Services  // 链式操作返回IServiceCollection
            .AddAppInsight(Configuration)  // 添加Application Insights服务
            .AddHealthChecks(Configuration)  // 添加健康检查服务
            .AddCustomMvc(Configuration)  // 添加自定义MVC配置
            .AddDevspaces()  // 添加开发空间支持
            .AddHttpClientServices(Configuration);  // 添加HTTP客户端服务

        // 显示个人身份信息（注意：生产环境中不应使用）
        IdentityModelEventSource.ShowPII = true;  // 警告！不要在生产环境中使用：https://aka.ms/IdentityModel/PII

        // 添加自定义身份验证服务
        services.AddCustomAuthentication(Configuration);
    }

    /// <summary>
    /// 配置HTTP请求处理管道
    /// 此方法由运行时调用，用于配置HTTP请求管道
    /// </summary>
    /// <param name="app">应用程序构建器</param>
    /// <param name="env">Web主机环境</param>
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // 移除默认的sub声明类型映射，以便使用原始的sub声明类型
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("sub");

        // 根据环境配置异常处理
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();  // 在开发环境中使用开发者异常页面
        }
        else
        {
            app.UseExceptionHandler("/Error");  // 在生产环境中使用错误处理页面
        }

        // 设置基本路径
        var pathBase = Configuration["PATH_BASE"];
        if (!string.IsNullOrEmpty(pathBase))
        {
            app.UsePathBase(pathBase);
        }

        app.UseStaticFiles();  // 启用静态文件服务
        app.UseSession();  // 启用会话

        // 初始化Web上下文种子数据
        WebContextSeed.Seed(app, env);

        // 修复当从本地docker-compose运行eShop时的samesite问题，因为默认使用http协议
        // 参考：https://github.com/dotnet-architecture/eShopOnContainers/issues/1391
        app.UseCookiePolicy(new CookiePolicyOptions { MinimumSameSitePolicy = AspNetCore.Http.SameSiteMode.Lax });

        app.UseRouting();  // 启用路由

        app.UseAuthentication();  // 启用身份验证
        app.UseAuthorization();  // 启用授权

        // 配置终结点
        app.UseEndpoints(endpoints =>
        {
            // 默认控制器路由
            endpoints.MapControllerRoute("default", "{controller=Catalog}/{action=Index}/{id?}");
            // 默认错误控制器路由
            endpoints.MapControllerRoute("defaultError", "{controller=Error}/{action=Error}");
            // 启用属性路由
            endpoints.MapControllers();
            // 存活探针健康检查
            endpoints.MapHealthChecks("/liveness", new HealthCheckOptions
            {
                Predicate = r => r.Name.Contains("self")
            });
            // 常规健康检查
            endpoints.MapHealthChecks("/hc", new HealthCheckOptions()
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
        });
    }
}

/// <summary>
/// 服务集合扩展方法类
/// 提供添加各种服务到IoC容器的扩展方法
/// </summary>
static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加Application Insights遥测服务
    /// </summary>
    public static IServiceCollection AddAppInsight(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddApplicationInsightsTelemetry(configuration);
        services.AddApplicationInsightsKubernetesEnricher();

        return services;
    }

    /// <summary>
    /// 添加健康检查服务
    /// </summary>
    public static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy())  // 添加自检健康检查
            .AddUrlGroup(new Uri(configuration["IdentityUrlHC"]), name: "identityapi-check", tags: new string[] { "identityapi" });  // 添加Identity API健康检查

        return services;
    }

    /// <summary>
    /// 添加自定义MVC配置服务
    /// </summary>
    public static IServiceCollection AddCustomMvc(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions();  // 添加选项模式支持
        services.Configure<AppSettings>(configuration);  // 配置应用设置
        services.AddSession();  // 添加会话服务
        services.AddDistributedMemoryCache();  // 添加分布式内存缓存

        // 如果在集群环境中运行，配置数据保护
        if (configuration.GetValue<string>("IsClusterEnv") == bool.TrueString)
        {
            services.AddDataProtection(opts =>
            {
                opts.ApplicationDiscriminator = "eshop.webmvc";  // 设置应用程序标识符
            })
            .PersistKeysToStackExchangeRedis(ConnectionMultiplexer.Connect(configuration["DPConnectionString"]), "DataProtection-Keys");  // 将密钥持久化到Redis
        }

        return services;
    }

    /// <summary>
    /// 添加所有HTTP客户端服务
    /// </summary>
    public static IServiceCollection AddHttpClientServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 添加HTTP上下文访问器
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        // 注册委托处理程序
        services.AddTransient<HttpClientAuthorizationDelegatingHandler>();
        services.AddTransient<HttpClientRequestIdDelegatingHandler>();

        // 设置每个HttpMessageHandler在池中的生命周期为5分钟
        services.AddHttpClient("extendedhandlerlifetime").SetHandlerLifetime(TimeSpan.FromMinutes(5)).AddDevspacesSupport();

        // 添加HTTP客户端服务
        // 购物篮服务
        services.AddHttpClient<IBasketService, BasketService>()
                .SetHandlerLifetime(TimeSpan.FromMinutes(5))  // 示例。默认生命周期为2分钟
                .AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>()
                .AddDevspacesSupport();

        // 商品目录服务
        services.AddHttpClient<ICatalogService, CatalogService>()
                .AddDevspacesSupport();

        // 订单服务
        services.AddHttpClient<IOrderingService, OrderingService>()
                .AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>()
                .AddHttpMessageHandler<HttpClientRequestIdDelegatingHandler>()
                .AddDevspacesSupport();

        // 添加自定义应用程序服务
        services.AddTransient<IIdentityParser<ApplicationUser>, IdentityParser>();

        return services;
    }

    /// <summary>
    /// 添加自定义身份验证服务
    /// </summary>
    public static IServiceCollection AddCustomAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var identityUrl = configuration.GetValue<string>("IdentityUrl");  // 身份服务URL
        var callBackUrl = configuration.GetValue<string>("CallBackUrl");  // 回调URL
        var sessionCookieLifetime = configuration.GetValue("SessionCookieLifetimeMinutes", 60);  // 会话Cookie生命周期

        // 添加身份验证服务          
        services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;  // 默认使用Cookie认证
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;  // 默认质询方案为OpenId Connect
        })
        .AddCookie(setup => setup.ExpireTimeSpan = TimeSpan.FromMinutes(sessionCookieLifetime))  // 添加Cookie认证
        .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>  // 添加OpenId Connect认证
        {
            options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.Authority = identityUrl.ToString();  // 身份服务器地址
            options.SignedOutRedirectUri = callBackUrl.ToString();  // 登出后重定向地址
            options.ClientId = "mvc";  // 客户端ID
            options.ClientSecret = "secret";  // 客户端密钥
            options.ResponseType = "code";  // 响应类型为授权码
            options.SaveTokens = true;  // 保存令牌
            options.GetClaimsFromUserInfoEndpoint = true;  // 从用户信息端点获取声明
            options.RequireHttpsMetadata = false;  // 不需要HTTPS元数据
            // 添加各种作用域
            options.Scope.Add("openid");
            options.Scope.Add("profile");
            options.Scope.Add("orders");
            options.Scope.Add("basket");
            options.Scope.Add("webshoppingagg");
            options.Scope.Add("orders.signalrhub");
            options.Scope.Add("webhooks");
        });

        return services;
    }
}
