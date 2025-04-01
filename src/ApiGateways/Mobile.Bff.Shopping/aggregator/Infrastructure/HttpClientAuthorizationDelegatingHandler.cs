namespace Microsoft.eShopOnContainers.Mobile.Shopping.HttpAggregator.Infrastructure;

/// <summary>
/// 授权委托处理程序，通过继承 DelegatingHandler 对 HTTP 请求添加授权头信息。
/// </summary>
public class HttpClientAuthorizationDelegatingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<HttpClientAuthorizationDelegatingHandler> _logger;

    /// <summary>
    /// 构造函数，通过依赖注入注入 IHttpContextAccessor 和 ILogger 实例。
    /// </summary>
    /// <param name="httpContextAccessor">用于访问当前 HTTP 上下文</param>
    /// <param name="logger">用于记录日志</param>
    public HttpClientAuthorizationDelegatingHandler(IHttpContextAccessor httpContextAccessor, ILogger<HttpClientAuthorizationDelegatingHandler> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    /// <summary>
    /// 重写 SendAsync 方法，在发送 HTTP 请求之前添加授权头信息。
    /// </summary>
    /// <param name="request">待发送的 HTTP 请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>返回 HTTP 响应</returns>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // 设置请求协议版本为 HTTP/2.0
        request.Version = new System.Version(2, 0);
        // 强制使用 GET 请求方法
        request.Method = HttpMethod.Get;

        // 从当前 HTTP 上下文获取 "Authorization" 头部
        var authorizationHeader = _httpContextAccessor.HttpContext.Request.Headers["Authorization"];

        // 如果 Authorization 头部不为空，则将其添加到请求头中
        if (!string.IsNullOrEmpty(authorizationHeader))
        {
            request.Headers.Add("Authorization", new List<string>() { authorizationHeader });
        }

        // 异步获取访问令牌
        var token = await GetToken();

        // 如果令牌存在，将其以 Bearer Token 的方式添加到请求头中
        if (token != null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        // 调用基础的 SendAsync 方法继续处理 HTTP 请求
        return await base.SendAsync(request, cancellationToken);
    }

    /// <summary>
    /// 异步方法，从当前 HTTP 上下文中获取访问令牌。
    /// </summary>
    /// <returns>返回令牌字符串，如果不存在则返回 null</returns>
    async Task<string> GetToken()
    {
        // 定义令牌在 HTTP 上下文中的键名
        const string ACCESS_TOKEN = "access_token";

        // 使用 GetTokenAsync 方法从 HTTP 上下文中获取令牌
        return await _httpContextAccessor.HttpContext.GetTokenAsync(ACCESS_TOKEN);
    }
}
