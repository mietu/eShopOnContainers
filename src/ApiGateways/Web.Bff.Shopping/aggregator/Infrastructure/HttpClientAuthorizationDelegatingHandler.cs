namespace Microsoft.eShopOnContainers.Web.Shopping.HttpAggregator.Infrastructure;

// 这是一个继承自DelegatingHandler的委托处理程序，用于在HTTP请求中添加认证令牌
public class HttpClientAuthorizationDelegatingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor; // 用于访问当前HTTP上下文

    // 构造函数，通过依赖注入注入IHttpContextAccessor
    public HttpClientAuthorizationDelegatingHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // 重写SendAsync方法，在发送请求前添加认证相关的头部信息
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // 从当前HTTP上下文中获取"Authorization"请求头
        var authorizationHeader = _httpContextAccessor.HttpContext
            .Request.Headers["Authorization"];

        // 如果Authorization头不为空，则将其添加到请求头中
        if (!string.IsNullOrWhiteSpace(authorizationHeader))
        {
            request.Headers.Add("Authorization", new List<string>() { authorizationHeader });
        }

        // 获取令牌字符串
        var token = await GetTokenAsync();

        // 如果令牌不为空，则将其作为Bearer类型的认证头添加到请求中
        if (token != null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        // 调用基类的SendAsync方法，继续发送HTTP请求
        return await base.SendAsync(request, cancellationToken);
    }

    // 异步方法，用于从当前HTTP上下文中获取认证令牌
    Task<string> GetTokenAsync()
    {
        // 定义令牌名称为"access_token"
        const string ACCESS_TOKEN = "access_token";

        // 利用扩展方法从当前HTTP上下文中获取access_token
        return _httpContextAccessor.HttpContext
            .GetTokenAsync(ACCESS_TOKEN);
    }
}
