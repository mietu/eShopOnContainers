namespace IdentityServerHost.Quickstart.UI;

// 静态扩展方法类，向相关类型添加额外的方法
public static class Extensions
{
    /// <summary>
    /// 检查重定向 URI 是否是原生客户端使用的。
    /// 如果 URI 既不以 "https" 开头也不以 "http" 开头，则认为是原生客户端。
    /// </summary>
    /// <param name="context">包含重定向 URI 的授权请求上下文</param>
    /// <returns>如果是原生客户端则返回 true，否则返回 false</returns>
    public static bool IsNativeClient(this AuthorizationRequest context)
    {
        // 判断 context.RedirectUri 是否不是以 "https" 或 "http" 开头
        return !context.RedirectUri.StartsWith("https", StringComparison.Ordinal)
            && !context.RedirectUri.StartsWith("http", StringComparison.Ordinal);
    }

    /// <summary>
    /// 返回一个加载页面视图，该视图会重定向到指定的 URI。
    /// 同时设置响应中的状态码和 Location header
    /// </summary>
    /// <param name="controller">目标控制器，便于调用 View 方法</param>
    /// <param name="viewName">视图名称</param>
    /// <param name="redirectUri">最终重定向的 URI</param>
    /// <returns>返回一个包含重定向视图模型的 IActionResult</returns>
    public static IActionResult LoadingPage(this Controller controller, string viewName, string redirectUri)
    {
        // 设置响应状态码为 200
        controller.HttpContext.Response.StatusCode = 200;
        // 清空 Location 响应头
        controller.HttpContext.Response.Headers["Location"] = "";

        // 返回视图，并将重定向 URI 封装到 RedirectViewModel 中
        return controller.View(viewName, new RedirectViewModel { RedirectUrl = redirectUri });
    }
}
