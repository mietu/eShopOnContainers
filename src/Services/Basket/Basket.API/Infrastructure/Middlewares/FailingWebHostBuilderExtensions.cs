namespace Basket.API.Infrastructure.Middlewares;

// 扩展方法类，用于扩展 IWebHostBuilder 的功能
public static class WebHostBuildertExtensions
{
    /// <summary>
    /// 扩展方法，注册 FailingStartupFilter 到 IWebHostBuilder 的服务容器中。
    /// 使用此方法可以为应用程序配置启动时的自定义处理逻辑。
    /// </summary>
    /// <param name="builder">当前的 IWebHostBuilder 实例</param>
    /// <param name="options">用于配置 FailingOptions 的委托参数</param>
    /// <returns>返回修改后的 IWebHostBuilder 实例</returns>
    public static IWebHostBuilder UseFailing(this IWebHostBuilder builder, Action<FailingOptions> options)
    {
        // 使用 ConfigureServices 配置服务容器
        builder.ConfigureServices(services =>
        {
            // 添加单例服务，注册 IStartupFilter 接口的实现 FailingStartupFilter，
            // 配置方法 options 将在 FailingStartupFilter 中应用
            services.AddSingleton<IStartupFilter>(new FailingStartupFilter(options));
        });
        // 返回 builder，以便可以链式调用其他扩展方法
        return builder;
    }
}

