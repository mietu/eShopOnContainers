namespace Basket.API.Infrastructure.Middlewares;

using Microsoft.Extensions.Logging;

public class FailingMiddleware
{
    // 定义下一个中间件委托
    private readonly RequestDelegate _next;
    // 用于指示是否应返回失败响应的标志
    private bool _mustFail;
    // 配置参数
    private readonly FailingOptions _options;
    // 日志记录器
    private readonly ILogger _logger;

    // 构造函数，将下一个委托、日志和配置参数注入
    public FailingMiddleware(RequestDelegate next, ILogger<FailingMiddleware> logger, FailingOptions options)
    {
        _next = next;
        _options = options;
        _mustFail = false;
        _logger = logger;
    }

    // 中间件入口方法
    public async Task Invoke(HttpContext context)
    {
        // 获取请求路径
        var path = context.Request.Path;
        // 如果路径等于配置的控制路径，则处理配置请求
        if (path.Equals(_options.ConfigPath, StringComparison.OrdinalIgnoreCase))
        {
            await ProcessConfigRequest(context);
            return;
        }

        // 如果满足失败条件，则返回500错误
        if (MustFail(context))
        {
            _logger.LogInformation("Response for path {Path} will fail.", path);
            context.Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;
            context.Response.ContentType = "text/plain";
            await context.Response.WriteAsync("Failed due to FailingMiddleware enabled.");
        }
        else
        {
            // 否则传递到下一个中间件
            await _next.Invoke(context);
        }
    }

    // 处理配置请求的逻辑，通过查询参数“enable”或“disable”来控制中间件行为
    private async Task ProcessConfigRequest(HttpContext context)
    {
        // 检查是否包含启用或禁用的查询参数
        var enable = context.Request.Query.Keys.Any(k => k == "enable");
        var disable = context.Request.Query.Keys.Any(k => k == "disable");

        // 如果同时包含两个参数，抛出异常
        if (enable && disable)
        {
            throw new ArgumentException("Must use enable or disable querystring values, but not both");
        }

        // 如果包含禁用参数，设置_mustFail为false，并返回 OK 响应信息
        if (disable)
        {
            _mustFail = false;
            await SendOkResponse(context, "FailingMiddleware disabled. Further requests will be processed.");
            return;
        }
        // 如果包含启用参数，设置_mustFail为true，并返回 OK 响应信息
        if (enable)
        {
            _mustFail = true;
            await SendOkResponse(context, "FailingMiddleware enabled. Further requests will return HTTP 500");
            return;
        }

        // 如果没有有效的查询参数，则返回当前状态信息
        await SendOkResponse(context, string.Format("FailingMiddleware is {0}", _mustFail ? "enabled" : "disabled"));
        return;
    }

    // 发送200 OK响应，包含指定的消息
    private async Task SendOkResponse(HttpContext context, string message)
    {
        context.Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
        context.Response.ContentType = "text/plain";
        await context.Response.WriteAsync(message);
    }

    // 判断是否需要失败返回
    private bool MustFail(HttpContext context)
    {
        // 获取请求路径值
        var rpath = context.Request.Path.Value;

        // 如果请求路径在不需要过滤的路径列表中，则不会失败
        if (_options.NotFilteredPaths.Any(p => p.Equals(rpath, StringComparison.InvariantCultureIgnoreCase)))
        {
            return false;
        }

        // 如果_mustFail为true，并且请求路径在配置的端点路径中或者端点路径列表为空，则返回失败
        return _mustFail &&
            (_options.EndpointPaths.Any(x => x == rpath)
            || _options.EndpointPaths.Count == 0);
    }
}
