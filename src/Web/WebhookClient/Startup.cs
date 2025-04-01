namespace WebhookClient;

/// <summary>
/// 应用程序的启动配置类，负责服务注册和HTTP请求管道配置
/// </summary>
public class Startup
{
    /// <summary>
    /// 构造函数，接收应用程序配置
    /// </summary>
    /// <param name="configuration">应用程序配置接口</param>
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
    /// 此方法由运行时调用，用于向DI容器添加服务
    /// </summary>
    /// <param name="services">服务集合</param>
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSession(opt =>
            {
                // 配置会话Cookie名称
                opt.Cookie.Name = ".eShopWebhooks.Session";
            })
            // 注册应用配置服务
            .AddConfiguration(Configuration)
            // 注册HTTP客户端服务
            .AddHttpClientServices(Configuration)
            // 注册自定义认证服务
            .AddCustomAuthentication(Configuration)
            // 注册Webhook客户端服务（单例模式）
            .AddTransient<IWebhooksClient, WebhooksClient>()
            // 注册Hook仓储服务（内存实现，单例模式）
            .AddSingleton<IHooksRepository, InMemoryHooksRepository>()
            // 添加MVC服务
            .AddMvc();

        // 添加API控制器服务
        services.AddControllers();
    }

    /// <summary>
    /// 配置HTTP请求处理管道
    /// 此方法由运行时调用，用于配置HTTP请求管道
    /// </summary>
    /// <param name="app">应用程序构建器</param>
    /// <param name="env">Web主机环境</param>
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // 配置应用路径基础
        var pathBase = Configuration["PATH_BASE"];
        if (!string.IsNullOrEmpty(pathBase))
        {
            app.UsePathBase(pathBase);
        }

        // 根据环境配置异常处理
        if (env.IsDevelopment())
        {
            // 开发环境显示详细异常页面
            app.UseDeveloperExceptionPage();
        }
        else
        {
            // 生产环境使用友好错误页
            app.UseExceptionHandler("/Error");
            // HSTS默认值为30天，生产环境可能需要调整，参见：https://aka.ms/aspnetcore-hsts
        }

        // 配置Webhook验证端点
        app.Map("/check", capp =>
        {
            capp.Run(async (context) =>
            {
                // 只处理OPTIONS请求，这是Webhook验证的标准方式
                if ("OPTIONS".Equals(context.Request.Method, StringComparison.InvariantCultureIgnoreCase))
                {
                    // 检查是否需要验证令牌
                    var validateToken = bool.TrueString.Equals(Configuration["ValidateToken"], StringComparison.InvariantCultureIgnoreCase);
                    var header = context.Request.Headers[HeaderNames.WebHookCheckHeader];
                    var value = header.FirstOrDefault();
                    var tokenToValidate = Configuration["Token"];

                    // 验证令牌或跳过验证（根据配置）
                    if (!validateToken || value == tokenToValidate)
                    {
                        // 如果有令牌，将其添加到响应头
                        if (!string.IsNullOrWhiteSpace(tokenToValidate))
                        {
                            context.Response.Headers.Add(HeaderNames.WebHookCheckHeader, tokenToValidate);
                        }
                        context.Response.StatusCode = (int)HttpStatusCode.OK;
                    }
                    else
                    {
                        // 令牌验证失败
                        await context.Response.WriteAsync("Invalid token");
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    }
                }
                else
                {
                    // 非OPTIONS请求返回400错误
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                }
            });
        });

        // 修复在docker-compose本地运行eShop时的samesite问题（HTTP协议）
        // 参考：https://github.com/dotnet-architecture/eShopOnContainers/issues/1391
        app.UseCookiePolicy(new CookiePolicyOptions { MinimumSameSitePolicy = SameSiteMode.Lax });

        // 配置静态文件中间件
        app.UseStaticFiles();
        // 启用会话中间件
        app.UseSession();
        // 配置路由中间件
        app.UseRouting();
        // 配置认证中间件
        app.UseAuthentication();
        // 配置授权中间件
        app.UseAuthorization();
        // 配置终结点
        app.UseEndpoints(endpoints =>
        {
            // 配置默认控制器路由
            endpoints.MapDefaultControllerRoute();
            // 配置Razor页面
            endpoints.MapRazorPages();
        });
    }
}

/// <summary>
/// 服务扩展方法类
/// 提供将各种服务注册到DI容器的扩展方法
/// </summary>
static class ServiceExtensions
{
    /// <summary>
    /// 添加应用程序配置服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        // 添加Options框架服务
        services.AddOptions();
        // 配置Settings类与配置绑定
        services.Configure<Settings>(configuration);
        return services;
    }

    /// <summary>
    /// 添加自定义认证服务
    /// 配置基于OpenID Connect的认证
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddCustomAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        // 从配置中获取身份认证URL和回调URL
        var identityUrl = configuration.GetValue<string>("IdentityUrl");
        var callBackUrl = configuration.GetValue<string>("CallBackUrl");

        // 添加认证服务
        services.AddAuthentication(options =>
        {
            // 默认方案为Cookie认证
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            // 默认挑战方案为OpenID Connect
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
        // 添加Cookie认证，设置过期时间为2小时
        .AddCookie(setup => setup.ExpireTimeSpan = TimeSpan.FromHours(2))
        // 添加OpenID Connect认证
        .AddOpenIdConnect(options =>
        {
            // 配置签入方案为Cookie
            options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            // 认证服务器地址
            options.Authority = identityUrl.ToString();
            // 登出后重定向URL
            options.SignedOutRedirectUri = callBackUrl.ToString();
            // 客户端ID
            options.ClientId = "webhooksclient";
            // 客户端密钥
            options.ClientSecret = "secret";
            // 响应类型为代码
            options.ResponseType = "code";
            // 保存令牌
            options.SaveTokens = true;
            // 从用户信息端点获取声明
            options.GetClaimsFromUserInfoEndpoint = true;
            // 不要求HTTPS元数据
            options.RequireHttpsMetadata = false;
            // 添加作用域
            options.Scope.Add("openid");
            options.Scope.Add("webhooks");
        });

        return services;
    }

    /// <summary>
    /// 添加HTTP客户端服务
    /// 配置命名的HTTP客户端和授权处理
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddHttpClientServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 添加HTTP上下文访问器
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        // 添加HTTP客户端授权委托处理器
        services.AddTransient<HttpClientAuthorizationDelegatingHandler>();
        // 添加具有扩展处理程序生命周期的HTTP客户端
        services.AddHttpClient("extendedhandlerlifetime").SetHandlerLifetime(Timeout.InfiniteTimeSpan);

        // 添加授权客户端HTTP服务
        services.AddHttpClient("GrantClient")
                // 设置处理程序生命周期为5分钟
                .SetHandlerLifetime(TimeSpan.FromMinutes(5))
                // 添加授权处理程序
                .AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>();

        return services;
    }
}
