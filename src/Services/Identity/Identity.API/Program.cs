// 设置应用名称
var appName = "Identity.API";

// 创建 Web 应用构建器
var builder = WebApplication.CreateBuilder();

// 添加自定义配置、Serilog 日志、MVC、数据库、身份认证、Identity Server、身份验证、健康检查和应用服务
builder.AddCustomConfiguration();
builder.AddCustomSerilog();
builder.AddCustomMvc();
builder.AddCustomDatabase();
builder.AddCustomIdentity();
builder.AddCustomIdentityServer();
builder.AddCustomAuthentication();
builder.AddCustomHealthChecks();
builder.AddCustomApplicationServices();

// 构建 Web 应用对象
var app = builder.Build();

// 如处于开发环境，则启用开发者异常页面，便于调试
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// 根据配置检查并设置基础路径（Path Base）
var pathBase = builder.Configuration["PATH_BASE"];
if (!string.IsNullOrEmpty(pathBase))
{
    app.UsePathBase(pathBase);
}

// 启用静态文件中间件以提供静态资源
app.UseStaticFiles();

// 配置 Cookie 策略，解决 Chrome 80+ 中 SameSite Cookie 的问题
app.UseCookiePolicy(new CookiePolicyOptions { MinimumSameSitePolicy = SameSiteMode.Lax });

// 启用路由中间件，用于请求路由分发
app.UseRouting();

// 启用 IdentityServer 中间件，支持 OAuth2 与 OpenID Connect 身份认证
app.UseIdentityServer();

// 启用授权中间件
app.UseAuthorization();

// 映射默认控制器路由
app.MapDefaultControllerRoute();

// 配置健康检查端点（"/hc"），显示所有注册的健康检查项目，自定义响应格式
app.MapHealthChecks("/hc", new HealthCheckOptions()
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

// 配置生命存活检查端点（"/liveness"），仅检查名称中包含 "self" 的健康检查项
app.MapHealthChecks("/liveness", new HealthCheckOptions
{
    Predicate = r => r.Name.Contains("self")
});

try
{
    // 记录日志，提示开始种子数据的初始化
    app.Logger.LogInformation("Seeding database ({ApplicationName})...", appName);

    // 创建作用域并自动应用数据库迁移与种子数据加载
    // 注意：这种方式不建议用于生产环境，建议使用生成 SQL 脚本的方式管理迁移
    using (var scope = app.Services.CreateScope())
    {
        await SeedData.EnsureSeedData(scope, app.Configuration, app.Logger);
    }

    // 记录日志，提示 Web 主机即将启动
    app.Logger.LogInformation("Starting web host ({ApplicationName})...", appName);
    // 启动 Web 应用
    app.Run();

    return 0;
}
catch (Exception ex)
{
    // 记录致命日志，提示服务异常退出
    app.Logger.LogCritical(ex, "Host terminated unexpectedly ({ApplicationName})...", appName);
    return 1;
}
finally
{
    // 在结束前关闭并刷新 Serilog 日志资源
    Serilog.Log.CloseAndFlush();
}

// 方法：获取程序配置对象
IConfiguration GetConfiguration()
{
    // 构建配置构建器，从当前目录加载 appsettings.json 文件和环境变量
    var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddEnvironmentVariables();

    // 构建初步配置对象
    var config = builder.Build();

    // 如果配置中启用 UseVault，则通过 Azure Key Vault 加载机密信息
    if (config.GetValue<bool>("UseVault", false))
    {
        TokenCredential credential = new ClientSecretCredential(
            config["Vault:TenantId"],
            config["Vault:ClientId"],
            config["Vault:ClientSecret"]);
        builder.AddAzureKeyVault(new Uri($"https://{config["Vault:Name"]}.vault.azure.net/"), credential);
    }

    // 返回最终构建的配置对象
    return builder.Build();
}