// 获取应用程序配置
var configuration = GetConfiguration();

// 创建并配置Serilog日志记录器
Log.Logger = CreateSerilogLogger(configuration);

try
{
    // 记录应用程序开始配置的日志
    Log.Information("Configuring web host ({ApplicationContext})...", Program.AppName);
    // 构建Web主机
    var host = BuildWebHost(configuration, args);

    // 记录应用程序开始运行的日志
    Log.Information("Starting web host ({ApplicationContext})...", Program.AppName);
    // 运行Web主机
    host.Run();

    return 0;
}
catch (Exception ex)
{
    // 记录未预期的终止异常
    Log.Fatal(ex, "Program terminated unexpectedly ({ApplicationContext})!", Program.AppName);
    return 1;
}
finally
{
    // 确保日志被关闭并刷新
    Log.CloseAndFlush();
}

// 构建Web主机的方法
#pragma warning disable CS0618 // 类型或成员已过时
IWebHost BuildWebHost(IConfiguration configuration, string[] args) =>
    WebHost.CreateDefaultBuilder(args)
        .CaptureStartupErrors(false)  // 不捕获启动错误（让它们直接抛出）
        .ConfigureAppConfiguration(x => x.AddConfiguration(configuration))  // 添加配置
        .UseStartup<Startup>()  // 使用Startup类配置服务和中间件
        .UseSerilog()  // 使用Serilog进行日志记录
        .Build();  // 构建Web主机
#pragma warning restore CS0618 // 类型或成员已过时

// 创建Serilog日志记录器的方法
Serilog.ILogger CreateSerilogLogger(IConfiguration configuration)
{
    // 从配置中获取Seq服务器URL和Logstash URL
    var seqServerUrl = configuration["Serilog:SeqServerUrl"];
    var logstashUrl = configuration["Serilog:LogstashgUrl"];

    // 创建日志配置
    var cfg = new LoggerConfiguration()
        .ReadFrom.Configuration(configuration)  // 从配置中读取Serilog配置
        .Enrich.WithProperty("ApplicationContext", Program.AppName)  // 添加应用程序上下文属性
        .Enrich.FromLogContext()  // 从日志上下文中丰富日志信息
        .WriteTo.Console();  // 将日志写入控制台

    // 如果提供了Seq服务器URL，则将日志写入Seq
    if (!string.IsNullOrWhiteSpace(seqServerUrl))
    {
        cfg.WriteTo.Seq(seqServerUrl);
    }

    // 如果提供了Logstash URL，则将日志以HTTP方式发送
    if (!string.IsNullOrWhiteSpace(logstashUrl))
    {
        cfg.WriteTo.Http(logstashUrl, null);
    }

    // 创建并返回日志记录器
    return cfg.CreateLogger();
}

// 获取应用程序配置的方法
IConfiguration GetConfiguration()
{
    // 创建配置构建器
    var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())  // 设置基础路径为当前目录
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)  // 添加appsettings.json配置文件
        .AddEnvironmentVariables();  // 添加环境变量

    // 构建并返回配置
    return builder.Build();
}

// Program类的部分定义
public partial class Program
{
    // 获取Startup类的命名空间
    private static readonly string _namespace = typeof(Startup).Namespace;

    // 从命名空间中提取应用程序名称
    // 逻辑是找到倒数第二个点之后的部分，例如从"Company.Project.WebMVC"中提取"Project.WebMVC"
    public static readonly string AppName = _namespace.Substring(_namespace.LastIndexOf('.', _namespace.LastIndexOf('.') - 1) + 1);
}