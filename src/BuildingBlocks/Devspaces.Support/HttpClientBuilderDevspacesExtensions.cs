namespace Devspaces.Support;

/// <summary>
/// 为 HttpClient 构建器添加 Devspaces 支持的扩展方法类
/// </summary>
public static class HttpClientBuilderDevspacesExtensions
{
    /// <summary>
    /// 扩展 IHttpClientBuilder，添加 DevspacesMessageHandler 到 HTTP 消息处理管道中
    /// </summary>
    /// <param name="builder">原始 IHttpClientBuilder 实例</param>
    /// <returns>扩展后的 IHttpClientBuilder 实例，便于链式调用</returns>
    public static IHttpClientBuilder AddDevspacesSupport(this IHttpClientBuilder builder)
    {
        // 将自定义的 HTTP 消息处理程序 DevspacesMessageHandler 添加到消息处理管道中
        builder.AddHttpMessageHandler<DevspacesMessageHandler>();
        // 返回扩展后的 builder 实例
        return builder;
    }
}
