// 获取应用程序配置
var configuration = GetConfiguration();

// 创建并配置Serilog日志记录器
Log.Logger = CreateSerilogLogger(configuration);

try
{
    // 记录配置Web主机的信息
    Log.Information("Configuring web host ({ApplicationContext})...", Program.AppName);
    // 构建Web主机
    var host = BuildWebHost(configuration, args);

    // 记录程序包版本信息
    LogPackagesVersionInfo();

    // 记录启动Web主机的信息
    Log.Information("Starting web host ({ApplicationContext})...", Program.AppName);
    // 运行Web主机
    host.Run();

    return 0;
}
catch (Exception ex)
{
    // 记录程序异常终止信息
    Log.Fatal(ex, "Program terminated unexpectedly ({ApplicationContext})!", Program.AppName);
    return 1;
}
finally
{
    // 关闭并刷新日志
    Log.CloseAndFlush();
}

/// <summary>
/// 构建Web主机
/// </summary>
/// <param name="configuration">应用程序配置</param>
/// <param name="args">命令行参数</param>
/// <returns>配置好的Web主机</returns>
IWebHost BuildWebHost(IConfiguration configuration, string[] args) =>
    WebHost.CreateDefaultBuilder(args)
        .CaptureStartupErrors(false)  // 不捕获启动错误
        .ConfigureAppConfiguration(x => x.AddConfiguration(configuration))  // 添加配置
        .UseStartup<Startup>()  // 使用Startup类配置服务和中间件
        .UseContentRoot(Directory.GetCurrentDirectory())  // 设置内容根目录
        .UseSerilog()  // 使用Serilog作为日志提供程序
        .Build();  // 构建Web主机

/// <summary>
/// 创建Serilog日志记录器
/// </summary>
/// <param name="configuration">应用程序配置</param>
/// <returns>配置好的Serilog日志记录器</returns>
Serilog.ILogger CreateSerilogLogger(IConfiguration configuration)
{
    // 从配置中获取Seq服务器URL
    var seqServerUrl = configuration["Serilog:SeqServerUrl"];
    // 从配置中获取Logstash URL
    var logstashUrl = configuration["Serilog:LogstashgUrl"];
    return new LoggerConfiguration()
        .MinimumLevel.Verbose()  // 设置最低日志级别为详细
        .Enrich.WithProperty("ApplicationContext", Program.AppName)  // 用应用程序名称丰富日志
        .Enrich.FromLogContext()  // 从日志上下文中丰富日志
        .WriteTo.Console()  // 写入控制台
        .WriteTo.Seq(string.IsNullOrWhiteSpace(seqServerUrl) ? "http://seq" : seqServerUrl)  // 写入Seq服务器
        .WriteTo.Http(string.IsNullOrWhiteSpace(logstashUrl) ? "http://logstash:8080" : logstashUrl, null)  // 写入Logstash
        .ReadFrom.Configuration(configuration)  // 从配置中读取其他设置
        .CreateLogger();  // 创建日志记录器
}

/// <summary>
/// 获取应用程序配置
/// </summary>
/// <returns>应用程序配置</returns>
IConfiguration GetConfiguration()
{
    // 创建配置构建器
    var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())  // 设置基础路径
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)  // 添加appsettings.json
        .AddEnvironmentVariables();  // 添加环境变量

    // 构建初始配置
    var config = builder.Build();

    // 如果启用了Azure Key Vault
    if (config.GetValue<bool>("UseVault", false))
    {
        // 创建客户端凭据
        TokenCredential credential = new ClientSecretCredential(
            config["Vault:TenantId"],
            config["Vault:ClientId"],
            config["Vault:ClientSecret"]);
        // 添加Azure Key Vault作为配置源
        builder.AddAzureKeyVault(new Uri($"https://{config["Vault:Name"]}.vault.azure.net/"), credential);
    }

    // 返回最终配置
    return builder.Build();
}

/// <summary>
/// 获取程序集版本信息
/// </summary>
/// <param name="assembly">目标程序集</param>
/// <returns>版本信息字符串</returns>
string GetVersion(Assembly assembly)
{
    try
    {
        // 获取文件版本和信息版本
        return $"{assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version} ({assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion.Split()[0]})";
    }
    catch
    {
        // 出错时返回空字符串
        return string.Empty;
    }
}

/// <summary>
/// 记录程序包版本信息
/// </summary>
void LogPackagesVersionInfo()
{
    var assemblies = new List<Assembly>();

    // 遍历当前程序集引用的所有程序集
    foreach (var dependencyName in typeof(Program).Assembly.GetReferencedAssemblies())
    {
        try
        {
            // 尝试加载引用的程序集
            assemblies.Add(Assembly.Load(dependencyName));
        }
        catch
        {
            // 加载失败则跳过
        }
    }

    // 生成版本列表
    var versionList = assemblies.Select(a => $"-{a.GetName().Name} - {GetVersion(a)}").OrderBy(value => value);

    // 记录版本信息
    Log.Logger.ForContext("PackageVersions", string.Join("\n", versionList)).Information("Package versions ({ApplicationContext})", Program.AppName);
}

/// <summary>
/// 程序类
/// </summary>
public partial class Program
{
    // 获取应用程序命名空间
    private static readonly string _namespace = typeof(Startup).Namespace;
    // 应用程序名称
    public static readonly string AppName = _namespace;
}