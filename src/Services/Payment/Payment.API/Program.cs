// 获取应用程序配置
var configuration = GetConfiguration();

// 创建并配置Serilog日志记录器
Log.Logger = CreateSerilogLogger(configuration);

try
{
    // 记录应用程序启动信息
    Log.Information("Configuring web host ({ApplicationContext})...", Program.AppName);
    // 构建Web主机
    var host = BuildWebHost(configuration, args);

    // 启动Web主机
    Log.Information("Starting web host ({ApplicationContext})...", Program.AppName);
    host.Run();

    return 0; // 正常退出返回0
}
catch (Exception ex)
{
    // 捕获并记录未处理的异常
    Log.Fatal(ex, "Program terminated unexpectedly ({ApplicationContext})!", Program.AppName);
    return 1; // 异常退出返回1
}
finally
{
    // 确保日志被刷新并关闭
    Log.CloseAndFlush();
}

// 构建Web主机的方法
#pragma warning disable CS0618 // 类型或成员已过时
IWebHost BuildWebHost(IConfiguration configuration, string[] args) =>
    WebHost.CreateDefaultBuilder(args) // 创建默认构建器
        .CaptureStartupErrors(false) // 不捕获启动错误(将直接抛出)
        .ConfigureAppConfiguration(x => x.AddConfiguration(configuration)) // 添加配置
        .UseStartup<Startup>() // 使用Startup类进行配置
        .UseContentRoot(Directory.GetCurrentDirectory()) // 设置内容根目录
        .UseSerilog() // 使用Serilog作为日志提供程序
        .Build(); // 构建Web主机
#pragma warning restore CS0618 // 类型或成员已过时

// 创建Serilog日志记录器的方法
Serilog.ILogger CreateSerilogLogger(IConfiguration configuration)
{
    // 从配置中获取Seq服务器和Logstash的URL
    var seqServerUrl = configuration["Serilog:SeqServerUrl"];
    var logstashUrl = configuration["Serilog:LogstashgUrl"];

    // 配置和创建日志记录器
    return new LoggerConfiguration()
        .MinimumLevel.Verbose() // 设置最低日志级别为Verbose
        .Enrich.WithProperty("ApplicationContext", Program.AppName) // 添加应用程序上下文属性
        .Enrich.FromLogContext() // 从日志上下文中添加属性
        .WriteTo.Console() // 输出到控制台
        .WriteTo.Seq(string.IsNullOrWhiteSpace(seqServerUrl) ? "http://seq" : seqServerUrl) // 输出到Seq服务器
        .WriteTo.Http(string.IsNullOrWhiteSpace(logstashUrl) ? "http://logstash:8080" : logstashUrl, null) // 输出到Logstash
        .ReadFrom.Configuration(configuration) // 从配置中读取额外设置
        .CreateLogger(); // 创建日志记录器
}

// 获取应用程序配置的方法
IConfiguration GetConfiguration()
{
    // 创建配置构建器
    var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory()) // 设置基础路径
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true) // 添加appsettings.json配置文件
        .AddEnvironmentVariables(); // 添加环境变量

    // 构建初始配置
    var config = builder.Build();

    // 如果配置指示使用Azure Key Vault
    if (config.GetValue<bool>("UseVault", false))
    {
        // 创建访问Azure Key Vault的凭据
        TokenCredential credential = new ClientSecretCredential(
            config["Vault:TenantId"],
            config["Vault:ClientId"],
            config["Vault:ClientSecret"]);
        // 添加Azure Key Vault作为配置源
        builder.AddAzureKeyVault(new Uri($"https://{config["Vault:Name"]}.vault.azure.net/"), credential);
    }

    // 返回完整配置
    return builder.Build();
}

// 程序类的部分定义
public partial class Program
{
    // 获取命名空间
    public static string Namespace = typeof(Startup).Namespace;
    // 从命名空间中提取应用程序名称
    // 假设命名空间格式为"X.Y.Z"，它将提取"Y.Z"部分
    public static string AppName = Namespace.Substring(Namespace.LastIndexOf('.', Namespace.LastIndexOf('.') - 1) + 1);
}