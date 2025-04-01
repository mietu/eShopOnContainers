namespace WebStatus;

/// <summary>
/// 启动类，负责配置应用程序的服务和HTTP请求处理管道
/// </summary>
public class Startup
{
    /// <summary>
    /// 构造函数，注入配置
    /// </summary>
    /// <param name="configuration">应用程序配置</param>
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    /// <summary>
    /// 应用程序配置属性
    /// </summary>
    public IConfiguration Configuration { get; }

    /// <summary>
    /// 配置应用程序的服务
    /// 此方法由运行时调用，用于向依赖注入容器中添加服务
    /// </summary>
    /// <param name="services">服务集合</param>
    public void ConfigureServices(IServiceCollection services)
    {
        // 注册Application Insights服务，用于监控和诊断
        RegisterAppInsights(services);

        // 添加MVC服务，支持控制器
        services.AddMvc();

        // 添加选项模式支持
        services.AddOptions();

        // 配置健康检查服务
        // 添加名为"self"的健康检查，始终返回健康状态
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy());

        // 添加健康检查UI服务
        // 使用内存存储方式存储健康检查数据
        services
            .AddHealthChecksUI()
            .AddInMemoryStorage();
    }

    /// <summary>
    /// 配置HTTP请求处理管道
    /// 此方法由运行时调用，用于配置HTTP请求处理管道
    /// </summary>
    /// <param name="app">应用程序构建器</param>
    /// <param name="env">web宿主环境</param>
    /// <param name="loggerFactory">日志工厂</param>
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
    {
        // 以下日志配置当前被注释
        //loggerFactory.AddAzureWebAppDiagnostics();
        //loggerFactory.AddApplicationInsights(app.ApplicationServices, LogLevel.Trace);

        // 配置环境特定的异常处理
        if (env.IsDevelopment())
        {
            // 在开发环境使用开发者异常页面
            app.UseDeveloperExceptionPage();
        }
        else
        {
            // 在生产环境使用异常处理中间件重定向到错误页面
            app.UseExceptionHandler("/Home/Error");
        }

        // 获取基础路径配置
        var pathBase = Configuration["PATH_BASE"];
        if (!string.IsNullOrEmpty(pathBase))
        {
            // 如果配置了基础路径，则使用该路径作为应用程序基础路径
            app.UsePathBase(pathBase);
        }

        // 配置健康检查UI中间件
        app.UseHealthChecksUI(config =>
        {
            // 配置资源路径，根据是否有基础路径进行调整
            config.ResourcesPath = string.IsNullOrEmpty(pathBase) ? "/ui/resources" : $"{pathBase}/ui/resources";
            // 配置UI访问路径
            config.UIPath = "/hc-ui";
        });

        // 启用静态文件服务
        app.UseStaticFiles();

        // 启用路由中间件
        app.UseRouting();

        // 配置端点
        app.UseEndpoints(endpoints =>
        {
            // 映射默认控制器路由
            endpoints.MapDefaultControllerRoute();

            // 映射健康检查端点到"/liveness"路径
            // 只包含名称包含"self"的健康检查
            endpoints.MapHealthChecks("/liveness", new HealthCheckOptions
            {
                Predicate = r => r.Name.Contains("self")
            });
        });
    }

    /// <summary>
    /// 注册Application Insights服务
    /// </summary>
    /// <param name="services">服务集合</param>
    private void RegisterAppInsights(IServiceCollection services)
    {
        // 添加Application Insights遥测服务
        services.AddApplicationInsightsTelemetry(Configuration);

        // 添加Kubernetes环境信息到Application Insights
        services.AddApplicationInsightsKubernetesEnricher();
    }
}
