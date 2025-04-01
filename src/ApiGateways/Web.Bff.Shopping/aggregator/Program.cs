// 异步运行 Web 应用程序
await BuildWebHost(args).RunAsync();

// BuildWebHost 方法用于创建和配置 IWebHost 对象
#pragma warning disable CS0618 // 类型或成员已过时
IWebHost BuildWebHost(string[] args) =>
    WebHost
        // 创建默认的 Web 主机构建器
        .CreateDefaultBuilder(args)
        // 自定义应用程序配置
        .ConfigureAppConfiguration(cb =>
        {
            // 获取当前配置源集合
            var sources = cb.Sources;
            // 在索引位置 3 插入自定义 JSON 配置源
            sources.Insert(3, new Microsoft.Extensions.Configuration.Json.JsonConfigurationSource()
            {
                // 指定该配置源为可选，即使文件不存在也不会报错
                Optional = true,
                // 配置文件名称
                Path = "appsettings.localhost.json",
                // 表示文件内容修改后不自动重载配置
                ReloadOnChange = false
            });
        })
        // 指定启动类，用于配置应用程序依赖项和请求管道
        .UseStartup<Startup>()
        // 使用 Serilog 进行日志配置
        .UseSerilog((builderContext, config) =>
        {
            config
                // 设置最低日志等级为 Information
                .MinimumLevel.Information()
                // 从上下文中提取日志相关信息
                .Enrich.FromLogContext()
                // 控制台输出日志
                .WriteTo.Console();
        })
        // 构建 IWebHost 实例
        .Build();
#pragma warning restore CS0618 // 类型或成员已过时