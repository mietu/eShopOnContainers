namespace WebMVC.Infrastructure;

/// <summary>
/// HTTP客户端授权委托处理程序，用于在向API发送请求时自动添加授权信息
/// 继承自DelegatingHandler以便可以拦截和修改HTTP请求
/// </summary>
public class HttpClientAuthorizationDelegatingHandler
    : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// 构造函数，注入HTTP上下文访问器
    /// </summary>
    /// <param name="httpContextAccessor">用于访问当前HTTP请求上下文</param>
    public HttpClientAuthorizationDelegatingHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// 重写SendAsync方法，在发送请求前添加授权信息
    /// </summary>
    /// <param name="request">要发送的HTTP请求</param>
    /// <param name="cancellationToken">取消操作的令牌</param>
    /// <returns>HTTP响应消息</returns>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // 从当前请求头中获取Authorization头信息
        var authorizationHeader = _httpContextAccessor.HttpContext
            .Request.Headers["Authorization"];

        // 如果当前请求包含Authorization头，则将其添加到发出的请求中
        if (!string.IsNullOrEmpty(authorizationHeader))
        {
            request.Headers.Add("Authorization", new List<string>() { authorizationHeader });
        }

        // 尝试从身份验证中间件获取访问令牌
        var token = await GetToken();

        // 如果获取到令牌，则以Bearer方式添加到请求头中
        if (token != null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        // 调用基类方法继续发送请求
        return await base.SendAsync(request, cancellationToken);
    }

    /// <summary>
    /// 从当前HTTP上下文中获取访问令牌
    /// </summary>
    /// <returns>访问令牌字符串，如果不存在则为null</returns>
    async Task<string> GetToken()
    {
        const string ACCESS_TOKEN = "access_token";

        // 使用身份验证扩展方法从HTTP上下文中获取令牌
        return await _httpContextAccessor.HttpContext
            .GetTokenAsync(ACCESS_TOKEN);
    }
}
