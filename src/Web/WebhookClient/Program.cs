// 创建Web主机构建器，构建并运行应用程序
CreateWebHostBuilder(args).Build().Run();


// 定义Web主机构建器创建方法
// 该方法使用默认设置创建WebHost，并使用Startup类进行配置
IWebHostBuilder CreateWebHostBuilder(string[] args) =>
    WebHost.CreateDefaultBuilder(args)
        .UseStartup<Startup>(); // 使用Startup类配置应用程序服务和请求管道
