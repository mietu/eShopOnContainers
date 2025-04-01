namespace WebhookClient;

/// <summary>
/// 授权委托处理程序，用于向外发请求添加授权信息
/// 此处理程序实现了两种授权方式：
/// 1. 从当前请求上下文中获取 Authorization 头并转发
/// 2. 从当前请求上下文中获取访问令牌并作为 Bearer 令牌添加
/// </summary>
public class HttpClientAuthorizationDelegatingHandler
        : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="httpContextAccessor">HTTP 上下文访问器，用于获取当前请求的上下文</param>
    public HttpClientAuthorizationDelegatingHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// 重写 SendAsync 方法，在发送请求前添加授权信息
    /// </summary>
    /// <param name="request">即将发送的 HTTP 请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>HTTP 响应消息</returns>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // 从当前请求中获取授权头
        var authorizationHeader = _httpContextAccessor.HttpContext
            .Request.Headers["Authorization"];

        // 如果存在授权头，则将其添加到发出的请求中
        if (!string.IsNullOrEmpty(authorizationHeader))
        {
            request.Headers.Add("Authorization", new List<string>() { authorizationHeader });
        }

        // 尝试获取访问令牌
        var token = await GetToken();

        // 如果获取到令牌，则将其添加为 Bearer 令牌
        if (token != null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        // 调用基类方法继续处理请求
        return await base.SendAsync(request, cancellationToken);
    }

    /// <summary>
    /// 从当前 HTTP 上下文中获取访问令牌
    /// </summary>
    /// <returns>访问令牌，如果不存在则返回 null</returns>
    private async Task<string> GetToken()
    {
        const string ACCESS_TOKEN = "access_token";

        // 使用 HttpContext 扩展方法获取令牌
        return await _httpContextAccessor.HttpContext
            .GetTokenAsync(ACCESS_TOKEN);
    }
}
