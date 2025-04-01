// 获取配置
var configuration = GetConfiguration();

// 创建并配置 Serilog 日志记录器，这里会根据配置文件中指定的日志目标生成记录器
Log.Logger = CreateSerilogLogger(configuration);

try
{
    // 记录信息：配置 Web 主机，使用 Program.AppName 表示应用程序上下文
    Log.Information("Configuring web host ({ApplicationContext})...", Program.AppName);
    // 根据配置和命令行参数构建 Web 主机
    var host = BuildWebHost(configuration, args);

    // 记录信息：应用数据库迁移，确保数据库模式是最新的
    Log.Information("Applying migrations ({ApplicationContext})...", Program.AppName);
    host.MigrateDbContext<OrderingContext>((context, services) =>
    {
        // 从服务容器中获取 Web 主机环境、应用设置及日志记录器实例
        var env = services.GetService<IWebHostEnvironment>();
        var settings = services.GetService<IOptions<OrderingSettings>>();
        var logger = services.GetService<ILogger<OrderingContextSeed>>();

        // 调用数据种子类进行数据初始化，此过程同步等待完成
        new OrderingContextSeed()
            .SeedAsync(context, env, settings, logger)
            .Wait();
    })
    // 迁移集成事件日志数据库上下文，示例中未进行额外配置
    .MigrateDbContext<IntegrationEventLogContext>((_, __) => { });

    // 记录信息：启动 Web 主机
    Log.Information("Starting web host ({ApplicationContext})...", Program.AppName);
    // 启动 Web 主机监听请求
    host.Run();

    return 0;
}
catch (Exception ex)
{
    // 记录致命错误信息，程序出现未捕获异常时记录日志并返回错误代码
    Log.Fatal(ex, "Program terminated unexpectedly ({ApplicationContext})!", Program.AppName);
    return 1;
}
finally
{
    // 确保程序退出前关闭并刷新日志记录器
    Log.CloseAndFlush();
}

/// <summary>
/// 构建 Web 主机的方法，根据传入的配置与命令行参数来设置 Kestrel 服务器以及其它配置项
/// </summary>
/// <param name="configuration">应用程序配置项</param>
/// <param name="args">命令行参数</param>
/// <returns>构建后的 IWebHost 实例</returns>
IWebHost BuildWebHost(IConfiguration configuration, string[] args) =>
    WebHost.CreateDefaultBuilder(args)
        // 禁用启动错误捕获，便于日志详细记录
        .CaptureStartupErrors(false)
        .ConfigureKestrel(options =>
        {
            // 获取在配置中定义的 HTTP 与 gRPC 端口
            var ports = GetDefinedPorts(configuration);
            // 配置 HTTP 监听，支持 HTTP/1 与 HTTP/2 协议
            options.Listen(IPAddress.Any, ports.httpPort, listenOptions =>
            {
                listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
            });

            // 配置 gRPC 监听，使用 HTTP/2 协议
            options.Listen(IPAddress.Any, ports.grpcPort, listenOptions =>
            {
                listenOptions.Protocols = HttpProtocols.Http2;
            });
        })
        // 将额外的配置合并到应用程序配置中
        .ConfigureAppConfiguration(x => x.AddConfiguration(configuration))
        // 指定 Startup 类来启动应用程序的配置、服务和中间件
        .UseStartup<Startup>()
        // 指定当前工作目录作为应用程序内容根目录
        .UseContentRoot(Directory.GetCurrentDirectory())
        // 使用 Serilog 做为日志记录器
        .UseSerilog()
        .Build();

/// <summary>
/// 根据配置创建 Serilog 日志记录器
/// </summary>
/// <param name="configuration">应用程序配置</param>
/// <returns>配置好的 Serilog.ILogger 实例</returns>
Serilog.ILogger CreateSerilogLogger(IConfiguration configuration)
{
    // 从配置中获取 Seq 和 Logstash 的 URL
    var seqServerUrl = configuration["Serilog:SeqServerUrl"];
    var logstashUrl = configuration["Serilog:LogstashgUrl"];
    return new LoggerConfiguration()
        // 设置最低日志级别
        .MinimumLevel.Verbose()
        // 添加应用上下文属性，便于日志信息关联
        .Enrich.WithProperty("ApplicationContext", Program.AppName)
        // 从日志上下文中提取额外信息
        .Enrich.FromLogContext()
        // 将日志写入控制台
        .WriteTo.Console()
        // 将日志发送至 Seq 服务器（如果未设置则采用默认地址）
        .WriteTo.Seq(string.IsNullOrWhiteSpace(seqServerUrl) ? "http://seq" : seqServerUrl)
        // 将日志发送至 Logstash 服务器（如果未设置则采用默认地址）
        .WriteTo.Http(string.IsNullOrWhiteSpace(logstashUrl) ? "http://logstash:8080" : logstashUrl, null)
        // 从配置中读取更多日志设置
        .ReadFrom.Configuration(configuration)
        .CreateLogger();
}

/// <summary>
/// 根据配置构造应用程序的配置对象
/// </summary>
/// <returns>构建后的 IConfiguration 实例</returns>
IConfiguration GetConfiguration()
{
    // 使用当前工作目录构造配置构建器，加载 appsettings.json 文件以及环境变量
    var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddEnvironmentVariables();

    // 构建初步的配置对象
    var config = builder.Build();

    // 如果配置中启用了 Vault，则使用 Azure Key Vault 进行配置值加载
    if (config.GetValue<bool>("UseVault", false))
    {
        // 使用客户端凭证认证获取 Azure Key Vault 凭据
        TokenCredential credential = new ClientSecretCredential(
            config["Vault:TenantId"],
            config["Vault:ClientId"],
            config["Vault:ClientSecret"]);
        // 添加 Azure Key Vault 作为配置源
        builder.AddAzureKeyVault(new Uri($"https://{config["Vault:Name"]}.vault.azure.net/"), credential);
    }

    // 返回构建后的配置对象
    return builder.Build();
}

/// <summary>
/// 从配置中提取 HTTP 与 gRPC 的监听端口
/// </summary>
/// <param name="config">应用程序配置</param>
/// <returns>包含 httpPort 与 grpcPort 的元组，分别表示 HTTP 和 gRPC 端口</returns>
(int httpPort, int grpcPort) GetDefinedPorts(IConfiguration config)
{
    // 从配置中获取 gRPC 端口，默认值为 5001
    var grpcPort = config.GetValue("GRPC_PORT", 5001);
    // 从配置中获取 HTTP 端口，默认值为 80
    var port = config.GetValue("PORT", 80);
    return (port, grpcPort);
}

/// <summary>
/// 程序主类，包含应用程序的静态属性
/// </summary>
public partial class Program
{
    // 根据 Startup 的命名空间确定当前应用的命名空间
    public static string Namespace = typeof(Startup).Namespace;
    // 从命名空间中提取应用名称，依据点符号进行子串截取
    public static string AppName = Namespace.Substring(Namespace.LastIndexOf('.', Namespace.LastIndexOf('.') - 1) + 1);
}