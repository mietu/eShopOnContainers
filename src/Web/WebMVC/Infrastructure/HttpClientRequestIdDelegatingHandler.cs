namespace WebMVC.Infrastructure;

/// <summary>
/// HTTP 客户端请求 ID 处理器，用于自动为 POST 和 PUT 请求添加唯一的请求 ID
/// </summary>
/// <remarks>
/// 这个处理器在 HTTP 请求管道中拦截请求，并在适当的情况下添加 x-requestid 头。
/// 这对于请求跟踪、日志关联和分布式系统中的问题诊断非常有用。
/// </remarks>
public class HttpClientRequestIdDelegatingHandler
    : DelegatingHandler
{
    /// <summary>
    /// 初始化 <see cref="HttpClientRequestIdDelegatingHandler"/> 类的新实例
    /// </summary>
    public HttpClientRequestIdDelegatingHandler()
    {
    }

    /// <summary>
    /// 异步发送 HTTP 请求到下一个处理器，并在需要时添加请求 ID
    /// </summary>
    /// <param name="request">要发送的 HTTP 请求消息</param>
    /// <param name="cancellationToken">可用于取消操作的取消标记</param>
    /// <returns>表示异步操作的任务，包含 HTTP 响应消息</returns>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // 仅对 POST 和 PUT 请求添加请求 ID
        if (request.Method == HttpMethod.Post || request.Method == HttpMethod.Put)
        {
            // 检查请求头中是否已经包含 x-requestid
            if (!request.Headers.Contains("x-requestid"))
            {
                // 添加一个新的唯一请求 ID
                request.Headers.Add("x-requestid", Guid.NewGuid().ToString());
            }
        }

        // 将请求传递给管道中的下一个处理器
        return await base.SendAsync(request, cancellationToken);
    }
}
