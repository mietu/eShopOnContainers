// 引入必要的命名空间
var configuration = GetConfiguration(); // 获取配置，包含appsettings.json、环境变量以及可能的Azure Key Vault配置

Log.Logger = CreateSerilogLogger(configuration); // 初始化Serilog日志记录器

try
{
    // 记录信息：正在配置Web主机
    Log.Information("Configuring web host ({ApplicationContext})...", Program.AppName);
    var host = BuildWebHost(configuration, args); // 根据配置构建Web主机

    // 记录信息：启动Web主机
    Log.Information("Starting web host ({ApplicationContext})...", Program.AppName);
    host.Run(); // 运行Web主机

    return 0; // 成功退出
}
catch (Exception ex)
{
    // 记录致命错误，退出码为1
    Log.Fatal(ex, "Program terminated unexpectedly ({ApplicationContext})!", Program.AppName);
    return 1;
}
finally
{
    // 关闭并刷新日志记录器
    Log.CloseAndFlush();
}

/// <summary>
/// 构建并配置Web主机
/// </summary>
/// <param name="configuration">应用程序配置</param>
/// <param name="args">命令行参数</param>
/// <returns>IWebHost 实例</returns>
#pragma warning disable CS0618 // 类型或成员已过时
IWebHost BuildWebHost(IConfiguration configuration, string[] args) =>
    WebHost.CreateDefaultBuilder(args)
        .CaptureStartupErrors(false)
        .ConfigureKestrel(options =>
        {
            var ports = GetDefinedPorts(configuration); // 获取端口配置
            // 配置 HTTP 端口与协议
            options.Listen(IPAddress.Any, ports.httpPort, listenOptions =>
            {
                listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
            });
            // 配置 gRPC 端口与协议，gRPC通常使用 HTTP/2
            options.Listen(IPAddress.Any, ports.grpcPort, listenOptions =>
            {
                listenOptions.Protocols = HttpProtocols.Http2;
            });
        })
        .ConfigureAppConfiguration(x => x.AddConfiguration(configuration)) // 添加预先构建的配置
        .UseFailing(options =>
        {
            // 使用失败注入中间件，配置其路径不被过滤
            options.ConfigPath = "/Failing";
            options.NotFilteredPaths.AddRange(new[] { "/hc", "/liveness" });
        })
        .UseStartup<Startup>() // 指定Startup类
        .UseContentRoot(Directory.GetCurrentDirectory()) // 设置内容根路径
        .UseSerilog() // 使用Serilog进行日志记录
        .Build(); // 构建Web主机
#pragma warning restore CS0618 // 类型或成员已过时

/// <summary>
/// 根据配置创建Serilog日志记录器
/// </summary>
/// <param name="configuration">应用程序配置</param>
/// <returns>Serilog ILogger 实例</returns>
Serilog.ILogger CreateSerilogLogger(IConfiguration configuration)
{
    // 从配置中读取Seq和Logstash地址
    var seqServerUrl = configuration["Serilog:SeqServerUrl"];
    var logstashUrl = configuration["Serilog:LogstashgUrl"];

    // 配置并创建Serilog日志记录器
    return new LoggerConfiguration()
        .MinimumLevel.Verbose()
        .Enrich.WithProperty("ApplicationContext", Program.AppName) // 添加应用上下文属性
        .Enrich.FromLogContext()
        .WriteTo.Console() // 输出到控制台
        .WriteTo.Seq(string.IsNullOrWhiteSpace(seqServerUrl) ? "http://seq" : seqServerUrl)
        .WriteTo.Http(string.IsNullOrWhiteSpace(logstashUrl) ? "http://logstash:8080" : logstashUrl, null)
        .ReadFrom.Configuration(configuration)
        .CreateLogger();
}

/// <summary>
/// 构建应用程序的配置，支持appsettings.json、环境变量及Azure Key Vault
/// </summary>
/// <returns>IConfiguration 实例</returns>
IConfiguration GetConfiguration()
{
    var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddEnvironmentVariables();

    var config = builder.Build();

    // 检查是否需要使用Azure Key Vault进行密钥管理
    if (config.GetValue<bool>("UseVault", false))
    {
        TokenCredential credential = new ClientSecretCredential(
            config["Vault:TenantId"],
            config["Vault:ClientId"],
            config["Vault:ClientSecret"]);
        // 添加Azure Key Vault作为配置源
        builder.AddAzureKeyVault(new Uri($"https://{config["Vault:Name"]}.vault.azure.net/"), credential);
    }

    return builder.Build(); // 返回构建后的配置
}

/// <summary>
/// 根据配置读取HTTP和gRPC端口
/// </summary>
/// <param name="config">应用程序配置</param>
/// <returns>包含HTTP端口和gRPC端口的元组</returns>
(int httpPort, int grpcPort) GetDefinedPorts(IConfiguration config)
{
    var grpcPort = config.GetValue("GRPC_PORT", 5001);
    var port = config.GetValue("PORT", 80);
    return (port, grpcPort);
}

/// <summary>
/// 程序的主入口部分，通过partial类组织
/// </summary>
public partial class Program
{
    // 通过Startup类命名空间生成应用名称
    public static string Namespace = typeof(Startup).Namespace;
    public static string AppName = Namespace.Substring(Namespace.LastIndexOf('.', Namespace.LastIndexOf('.') - 1) + 1);
}
