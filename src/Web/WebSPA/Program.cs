await BuildWebHost(args).RunAsync();

#pragma warning disable CS0618 // 类型或成员已过时
IWebHost BuildWebHost(string[] args) =>
    WebHost.CreateDefaultBuilder(args)
     .UseStartup<Startup>()
        .UseContentRoot(Directory.GetCurrentDirectory())
        .ConfigureAppConfiguration((builderContext, config) =>
        {
            config.AddEnvironmentVariables();
        })
        .ConfigureLogging((hostingContext, builder) =>
        {
            builder.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
            builder.AddConsole();
            builder.AddDebug();
            builder.AddAzureWebAppDiagnostics();
        })
        .UseSerilog((builderContext, config) =>
        {
            config
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .WriteTo.Seq("http://seq")
                .ReadFrom.Configuration(builderContext.Configuration)
                .WriteTo.Console();
        })
        .Build();
#pragma warning restore CS0618 // 类型或成员已过时
