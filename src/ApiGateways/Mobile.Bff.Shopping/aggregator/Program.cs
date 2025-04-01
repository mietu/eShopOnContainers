// 程序入口点，启动Web主机并开始运行异步任务
await BuildWebHost(args).RunAsync();

// 根据传入参数构建和配置Web主机
#pragma warning disable CS0618 // 类型或成员已过时
IWebHost BuildWebHost(string[] args) =>
    WebHost
        .CreateDefaultBuilder(args)
        // 配置应用程序的配置源
        .ConfigureAppConfiguration(cb =>
        {
            // 获取当前配置源列表
            var sources = cb.Sources;
            // 在索引3的位置插入一个新的 Json 配置源
            // 用于加载可选的本地配置文件 "appsettings.localhost.json"
            sources.Insert(3, new Microsoft.Extensions.Configuration.Json.JsonConfigurationSource()
            {
                Optional = true,       // 指定配置文件为可选项
                Path = "appsettings.localhost.json", // 配置文件路径
                ReloadOnChange = false // 不监控文件变化
            });
        })
        // 使用 Startup 类配置应用的服务和请求管道
        .UseStartup<Startup>()
        // 配置 Serilog 日志记录器
        .UseSerilog((builderContext, config) =>
        {
            config
                .MinimumLevel.Information()   // 设置最小日志记录级别为 Information
                .Enrich.FromLogContext()      // 从日志上下文中添加信息
                .WriteTo.Console();           // 将日志输出到控制台
        })
        .Build(); // 构建 IWebHost 实例
#pragma warning restore CS0618 // 类型或成员已过时