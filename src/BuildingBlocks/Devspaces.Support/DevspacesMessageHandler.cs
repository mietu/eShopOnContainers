namespace Devspaces.Support;

// DevspacesMessageHandler 负责在 HTTP 请求链中转发特定的头信息
public class DevspacesMessageHandler : DelegatingHandler
{
    // 定义一个常量，表示需要传递的头信息名称
    private const string DevspacesHeaderName = "azds-route-as";

    // 用于访问当前请求的 HTTP 上下文
    private readonly IHttpContextAccessor _httpContextAccessor;

    // 构造函数，注入 IHttpContextAccessor 以获取当前请求上下文
    public DevspacesMessageHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // 重写 SendAsync 方法，在请求转发前添加必要的头信息
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // 获取当前 HTTP 请求对象
        var req = _httpContextAccessor.HttpContext.Request;

        // 检查当前请求是否包含指定的头信息
        if (req.Headers.ContainsKey(DevspacesHeaderName))
        {
            // 将头信息添加到传入的 HttpRequestMessage 中
            // 将原请求中的头信息转换为 IEnumerable<string> 类型并添加
            request.Headers.Add(DevspacesHeaderName, req.Headers[DevspacesHeaderName] as IEnumerable<string>);
        }

        // 调用基类方法继续处理请求
        return base.SendAsync(request, cancellationToken);
    }
}
