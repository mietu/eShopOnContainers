// 解析并添加注释：此文件主要用于配置 Serilog 日志、构建并运行 Web 主机。
var configuration = GetConfiguration();

// 初始化 Serilog 日志记录器
Log.Logger = CreateSerilogLogger(configuration);

try
{
    // 输出配置信息，并构建 Web 主机
    Log.Information("Configuring web host ({ApplicationContext})...", Program.AppName);
    var host = BuildWebHost(configuration, args);

    // 启动 Web 主机
    Log.Information("Starting web host ({ApplicationContext})...", Program.AppName);
    host.Run();

    return 0;
}
catch (Exception ex)
{
    // 如果发生异常，记录致命错误并返回错误状态码
    Log.Fatal(ex, "Program terminated unexpectedly ({ApplicationContext})!", Program.AppName);
    return 1;
}
finally
{
    // 程序结束前关闭并刷新日志
    Log.CloseAndFlush();
}

/// <summary>
/// 构建并返回一个 IWebHost 实例
/// </summary>
/// <param name="configuration">应用程序配置</param>
/// <param name="args">命令行参数</param>
/// <returns>配置好的 IWebHost 实例</returns>
#pragma warning disable CS0618 // 类型或成员已过时
static IWebHost BuildWebHost(IConfiguration configuration, string[] args) =>
    WebHost.CreateDefaultBuilder(args)
        .CaptureStartupErrors(false) // 不捕获启动错误
        .ConfigureAppConfiguration(x => x.AddConfiguration(configuration)) // 添加应用配置
        .UseStartup<Startup>() // 指定 Startup 类
        .UseSerilog() // 使用 Serilog 作为日志框架
        .Build();
#pragma warning restore CS0618 // 类型或成员已过时

/// <summary>
/// 根据配置创建 Serilog 日志记录器
/// </summary>
/// <param name="configuration">应用程序配置</param>
/// <returns>配置好的 Serilog 记录器</returns>
static Serilog.ILogger CreateSerilogLogger(IConfiguration configuration)
{
    // 从配置中读取 Seq 和 Logstash 的 URL
    var seqServerUrl = configuration["Serilog:SeqServerUrl"];
    var logstashUrl = configuration["Serilog:LogstashgUrl"];
    return new LoggerConfiguration()
        .MinimumLevel.Verbose() // 设置最低日志级别为 Verbose
        .Enrich.WithProperty("ApplicationContext", Program.AppName) // 增加应用程序上下文属性
        .Enrich.FromLogContext() // 增加来自 LogContext 的属性
        .WriteTo.Console() // 写入控制台
        .WriteTo.Seq(string.IsNullOrWhiteSpace(seqServerUrl) ? "http://seq" : seqServerUrl) // 写入 Seq 日志服务器
        .WriteTo.Http(string.IsNullOrWhiteSpace(logstashUrl) ? "http://logstash:8080" : logstashUrl, null) // 写入 Logstash
        .ReadFrom.Configuration(configuration) // 读取配置中的其他日志配置
        .CreateLogger();
}

/// <summary>
/// 获取并构建应用程序配置
/// </summary>
/// <returns>构建好的配置对象</returns>
static IConfiguration GetConfiguration()
{
    var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory()) // 设置基础路径为当前工作目录
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true) // 加载 JSON 配置文件
        .AddEnvironmentVariables(); // 加载环境变量配置

    return builder.Build();
}

/// <summary>
/// 程序入口类部分定义
/// </summary>
public partial class Program
{
    // 获取 Startup 类所在命名空间
    public static string Namespace = typeof(Startup).Namespace;
    // 从命名空间中解析并提取应用程序名称
    public static string AppName = Namespace.Substring(Namespace.LastIndexOf('.', Namespace.LastIndexOf('.') - 1) + 1);
}