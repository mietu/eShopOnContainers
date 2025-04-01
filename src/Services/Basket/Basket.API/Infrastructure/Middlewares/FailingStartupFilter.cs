namespace Basket.API.Infrastructure.Middlewares;

// FailingStartupFilter 实现 IStartupFilter 接口，用于在应用启动时注册自定义中间件
public class FailingStartupFilter : IStartupFilter
{
    // 存储传入的配置选项，用于自定义 FailingMiddleware 的行为
    private readonly Action<FailingOptions> _options;

    // 构造函数：通过依赖注入传入一个配置选项委托
    public FailingStartupFilter(Action<FailingOptions> optionsAction)
    {
        _options = optionsAction;
    }

    // 实现 IStartupFilter 接口的 Configure 方法
    // next 参数代表应用中下一个中间件的配置方法
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        // 返回一个委托，该委托用于配置中间件
        return app =>
        {
            // 添加 FailingMiddleware 中间件到管道中，并传入配置选项
            app.UseFailingMiddleware(_options);
            // 调用下一中间件的配置
            next(app);
        };
    }
}

