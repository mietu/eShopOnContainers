namespace Basket.API.Infrastructure.Middlewares;

/// <summary>
/// 扩展方法类，用于添加“故障中间件”（FailingMiddleware）到应用程序管道中。
/// </summary>
public static class FailingMiddlewareAppBuilderExtensions
{
    /// <summary>
    /// 以默认配置将故障中间件添加到请求管道中。
    /// </summary>
    /// <param name="builder">IApplicationBuilder实例</param>
    /// <returns>返回修改后的IApplicationBuilder实例</returns>
    public static IApplicationBuilder UseFailingMiddleware(this IApplicationBuilder builder)
    {
        // 调用重载方法，没有自定义配置
        return UseFailingMiddleware(builder, null);
    }

    /// <summary>
    /// 将故障中间件添加到请求管道中，并允许通过Action配置故障选项。
    /// </summary>
    /// <param name="builder">IApplicationBuilder实例</param>
    /// <param name="action">配置FailingOptions选项的委托</param>
    /// <returns>返回修改后的IApplicationBuilder实例</returns>
    public static IApplicationBuilder UseFailingMiddleware(this IApplicationBuilder builder, Action<FailingOptions> action)
    {
        // 创建默认的FailureOptions实例，可用于存放相关配置（如ConfigPath、EndpointPaths等）
        var options = new FailingOptions();

        // 如果传入了自定义配置，则执行该委托，更新options
        action?.Invoke(options);

        // 将FailingMiddleware中间件添加到请求处理管道中，同时传入配置参数options
        builder.UseMiddleware<FailingMiddleware>(options);

        // 返回IApplicationBuilder，支持链式调用
        return builder;
    }
}

