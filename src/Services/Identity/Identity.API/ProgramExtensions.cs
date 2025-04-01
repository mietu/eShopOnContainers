using Serilog;

namespace Microsoft.eShopOnContainers.Services.Identity.API;

public static class ProgramExtensions
{
    private const string AppName = "Identity API"; // 应用名称，用于日志标识等用途

    /// <summary>
    /// 添加自定义配置 - 加载 appsettings.json、环境变量以及条件下的 Azure Key Vault 配置。
    /// </summary>
    public static void AddCustomConfiguration(this WebApplicationBuilder builder)
    {
        // 将通过 GetConfiguration() 构建的配置添加到 builder.Configuration 中
        builder.Configuration.AddConfiguration(GetConfiguration()).Build();
    }

    /// <summary>
    /// 配置 Serilog 日志记录，包括控制台、Seq 和 Logstash 输出。
    /// </summary>
    public static void AddCustomSerilog(this WebApplicationBuilder builder)
    {
        var seqServerUrl = builder.Configuration["SeqServerUrl"];
        var logstashUrl = builder.Configuration["LogstashgUrl"];

        // 构建 Serilog 日志记录器配置
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose() // 设置最小日志级别为 Verbose
            .Enrich.WithProperty("ApplicationContext", AppName) // 添加应用上下文属性
            .Enrich.FromLogContext() // 从当前上下文提取额外信息
            .WriteTo.Console() // 写入控制台
            .WriteTo.Seq(string.IsNullOrWhiteSpace(seqServerUrl) ? "http://seq" : seqServerUrl) // 写入 Seq
            .WriteTo.Http(string.IsNullOrWhiteSpace(logstashUrl) ? "http://localhost:8080" : logstashUrl, null) // 写入 Logstash
            .ReadFrom.Configuration(builder.Configuration) // 从配置文件中读取其他配置
            .CreateLogger();

        // 使用 Serilog 替换默认日志提供程序
        builder.Host.UseSerilog();
    }

    /// <summary>
    /// 配置 MVC 服务，包括控制器、视图和 Razor 页面。
    /// </summary>
    public static void AddCustomMvc(this WebApplicationBuilder builder)
    {
        // 注册添加控制器及视图服务
        builder.Services.AddControllersWithViews();
        // 注册 API 控制器服务
        builder.Services.AddControllers();
        // 注册 Razor 页面服务
        builder.Services.AddRazorPages();
    }

    /// <summary>
    /// 配置数据库上下文，使用 SQL Server 数据库连接。
    /// </summary>
    public static void AddCustomDatabase(this WebApplicationBuilder builder) =>
        builder.Services.AddDbContext<ApplicationDbContext>(
            options => options.UseSqlServer(builder.Configuration["ConnectionString"]));

    /// <summary>
    /// 配置 ASP.NET Core Identity，以支持用户身份验证与授权。
    /// </summary>
    public static void AddCustomIdentity(this WebApplicationBuilder builder)
    {
        builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                // 添加数据库存储，用于存储 Identity 数据
                .AddEntityFrameworkStores<ApplicationDbContext>()
                // 添加用于生成令牌的默认提供程序
                .AddDefaultTokenProviders();
    }

    /// <summary>
    /// 配置 IdentityServer，用于 OAuth2 和 OpenID Connect 认证的实现。
    /// 使用内存中的资源、客户端和作用域配置。
    /// </summary>
    public static void AddCustomIdentityServer(this WebApplicationBuilder builder)
    {
        var identityServerBuilder = builder.Services.AddIdentityServer(options =>
        {
            options.IssuerUri = "null"; // 发行者 URI，可以根据需要进行更改
            options.Authentication.CookieLifetime = TimeSpan.FromHours(2); // 设置 Cookie 有效期

            // 启用各种事件通知，用于调试和日志记录
            options.Events.RaiseErrorEvents = true;
            options.Events.RaiseInformationEvents = true;
            options.Events.RaiseFailureEvents = true;
            options.Events.RaiseSuccessEvents = true;
        })
                // 加载内存中配置的 Identity 资源
                .AddInMemoryIdentityResources(Config.GetResources())
                // 加载内存中配置的 API 作用域
                .AddInMemoryApiScopes(Config.GetApiScopes())
                // 加载内存中配置的 API 资源
                .AddInMemoryApiResources(Config.GetApis())
                // 加载内存中配置的客户端
                .AddInMemoryClients(Config.GetClients(builder.Configuration))
                // 结合 ASP.NET Core Identity
                .AddAspNetIdentity<ApplicationUser>();

        // 开发环境使用临时的开发签名凭证，不建议在生产环境中使用
        identityServerBuilder.AddDeveloperSigningCredential();
    }

    /// <summary>
    /// 配置基本的身份验证服务。
    /// </summary>
    public static void AddCustomAuthentication(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication();
    }

    /// <summary>
    /// 配置健康检查服务，包括自检以及对 SQL Server 数据库的检查。
    /// </summary>
    public static void AddCustomHealthChecks(this WebApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks()
                // 添加自检项
                .AddCheck("self", () => HealthCheckResult.Healthy())
                // 添加 SQL Server 连接检查，标签标记为 IdentityDB
                .AddSqlServer(builder.Configuration["ConnectionString"],
                    name: "IdentityDB-check",
                    tags: new string[] { "IdentityDB" });
    }

    /// <summary>
    /// 配置应用程序自定义服务，将一些特定服务注册到 DI 容器中。
    /// </summary>
    public static void AddCustomApplicationServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddTransient<IProfileService, ProfileService>();
        builder.Services.AddTransient<ILoginService<ApplicationUser>, EFLoginService>();
        builder.Services.AddTransient<IRedirectService, RedirectService>();
    }

    /// <summary>
    /// 构建自定义配置提供者，基于 JSON 文件和环境变量，并支持可选的 Azure KeyVault 集成。
    /// </summary>
    static IConfiguration GetConfiguration()
    {
        // 创建配置构造器，基于当前工作目录和 appsettings.json 文件
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();

        var config = builder.Build();

        // 如果配置中启用了 KeyVault 支持，则添加 Azure KeyVault
        if (config.GetValue<bool>("UseVault", false))
        {
            // 使用客户端凭证方式创建 KeyVault 凭证
            TokenCredential credential = new ClientSecretCredential(
                config["Vault:TenantId"],
                config["Vault:ClientId"],
                config["Vault:ClientSecret"]);
            // 添加 Azure KeyVault 为配置源
            builder.AddAzureKeyVault(new Uri($"https://{config["Vault:Name"]}.vault.azure.net/"), credential);
        }

        // 返回最终构建的配置
        return builder.Build();
    }
}
