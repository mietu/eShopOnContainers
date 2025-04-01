// 获取应用配置，通过读取 appsettings.json 和环境变量构建 IConfiguration 实例。
var configuration = GetConfiguration();

// 使用配置信息配置 Serilog 日志记录器，并赋值给 Log.Logger
Log.Logger = CreateSerilogLogger(configuration);

try
{
    // 记录日志：正在配置 Web 主机，输出当前应用上下文信息
    Log.Information("Configuring web host ({ApplicationContext})...", Program.AppName);
    // 根据传入的配置和命令行参数创建 Web 主机
    var host = CreateHostBuilder(configuration, args);

    // 记录日志：正在应用数据库迁移
    Log.Information("Applying migrations ({ApplicationContext})...", Program.AppName);
    // 对 CatalogContext 应用迁移，并执行数据库种子初始化
    host.MigrateDbContext<CatalogContext>((context, services) =>
    {
        // 从依赖注入中获取 Web 主机环境
        var env = services.GetService<IWebHostEnvironment>();
        // 获取 CatalogSettings 配置项实例
        var settings = services.GetService<IOptions<CatalogSettings>>();
        // 获取日志记录器
        var logger = services.GetService<ILogger<CatalogContextSeed>>();

        // 运行数据初始化方法，等待任务完成
        new CatalogContextSeed().SeedAsync(context, env, settings, logger).Wait();
    })
    // 对 IntegrationEventLogContext 应用迁移（不需要额外初始化）
    .MigrateDbContext<IntegrationEventLogContext>((_, __) => { });

    // 记录日志：启动 Web 主机
    Log.Information("Starting web host ({ApplicationContext})...", Program.AppName);
    // 启动应用，开始监听 Web 请求
    host.Run();

    // 如果正常运行则返回 0
    return 0;
}
catch (Exception ex)
{
    // 捕获异常并记录严重日志，同时返回错误代码 1
    Log.Fatal(ex, "Program terminated unexpectedly ({ApplicationContext})!", Program.AppName);
    return 1;
}
finally
{
    // 关闭和刷新日志，确保所有日志都输出
    Log.CloseAndFlush();
}

// 创建并返回 Web 主机，配置应用配置、错误处理、Kestrel 服务、启动类、内容根目录、静态文件根目录及日志
#pragma warning disable CS0618 // 类型或成员已过时
IWebHost CreateHostBuilder(IConfiguration configuration, string[] args) =>
  WebHost.CreateDefaultBuilder(args)
      // 将外部配置注入
      .ConfigureAppConfiguration(x => x.AddConfiguration(configuration))
      // 捕获启动时错误，可能用于调试目的
      .CaptureStartupErrors(false)
      // 配置 Kestrel 服务器的监听端口及协议
      .ConfigureKestrel(options =>
      {
          var ports = GetDefinedPorts(configuration);
          // 监听 HTTP 端口，同时支持 HTTP/1 和 HTTP/2 协议
          options.Listen(IPAddress.Any, ports.httpPort, listenOptions =>
          {
              listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
          });
          // 监听 gRPC 端口，仅支持 HTTP/2 协议
          options.Listen(IPAddress.Any, ports.grpcPort, listenOptions =>
          {
              listenOptions.Protocols = HttpProtocols.Http2;
          });
      })
      // 指定启动类 Startup
      .UseStartup<Startup>()
      // 设置当前工作目录为内容根目录
      .UseContentRoot(Directory.GetCurrentDirectory())
      // 使用 Pics 文件夹作为静态文件目录
      .UseWebRoot("Pics")
      // 使用 Serilog 作为日志提供器
      .UseSerilog()
      .Build();
#pragma warning restore CS0618 // 类型或成员已过时

// 根据配置信息创建 Serilog 日志记录器
Serilog.ILogger CreateSerilogLogger(IConfiguration configuration)
{
    // 从配置中读取 Seq 和 Logstash 的 URL
    var seqServerUrl = configuration["Serilog:SeqServerUrl"];
    var logstashUrl = configuration["Serilog:LogstashgUrl"];
    return new LoggerConfiguration()
        // 设置最低日志级别为 Verbose（最详细）
        .MinimumLevel.Verbose()
        // 注入应用上下文属性，方便后续日志筛选
        .Enrich.WithProperty("ApplicationContext", Program.AppName)
        // 从日志上下文中提取额外信息
        .Enrich.FromLogContext()
        // 向控制台输出日志
        .WriteTo.Console()
        // 将日志输出到 Seq，如果配置为空则使用默认 URL
        .WriteTo.Seq(string.IsNullOrWhiteSpace(seqServerUrl) ? "http://seq" : seqServerUrl)
        // 将日志输出到 Logstash，如果配置为空则使用默认 URL
        .WriteTo.Http(string.IsNullOrWhiteSpace(logstashUrl) ? "http://logstash:8080" : logstashUrl, null)
        // 从配置文件中读取更多日志配置
        .ReadFrom.Configuration(configuration)
        .CreateLogger();
}

// 根据配置获取定义的 HTTP 和 gRPC 端口
(int httpPort, int grpcPort) GetDefinedPorts(IConfiguration config)
{
    // 从配置读取 gRPC 端口，默认为 81
    var grpcPort = config.GetValue("GRPC_PORT", 81);
    // 从配置读取 HTTP 端口，默认为 80
    var port = config.GetValue("PORT", 80);
    return (port, grpcPort);
}

// 构建 IConfiguration 对象，读取 appsettings.json 和环境变量；如果配置中启用了 Vault，则可以扩展读取 Azure Key Vault
IConfiguration GetConfiguration()
{
    var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        // 添加 appsettings.json 文件，必须存在且支持热重载
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        // 添加环境变量
        .AddEnvironmentVariables();

    var config = builder.Build();

    // 检查是否需要使用 Vault 配置中心
    if (config.GetValue<bool>("UseVault", false))
    {
        // 使用 ClientSecretCredential 验证身份，准备访问 Azure Key Vault
        TokenCredential credential = new ClientSecretCredential(
            config["Vault:TenantId"],
            config["Vault:ClientId"],
            config["Vault:ClientSecret"]);
        // 可通过以下代码将 Azure Key Vault 集成到配置中，目前被注释掉了
        //builder.AddAzureKeyVault(new Uri($"https://{config["Vault:Name"]}.vault.azure.net/"), credential);        
    }

    // 重新构建并返回配置对象（注：如果添加 Azure Key Vault 则需要在此返回更新后的配置）
    return builder.Build();
}

// 定义 Program 部分类，并从 Startup 的命名空间中获取应用名称
public partial class Program
{
    // 存储 Startup 的命名空间
    public static string Namespace = typeof(Startup).Namespace;
    // 截取命名空间的部分作为应用名称，通过截取最后两个点之间的内容
    public static string AppName = Namespace.Substring(Namespace.LastIndexOf('.', Namespace.LastIndexOf('.') - 1) + 1);
}