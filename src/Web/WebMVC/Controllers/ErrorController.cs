namespace WebMVC.Controllers;

/// <summary>
/// 错误处理控制器
/// </summary>
/// <remarks>
/// 该控制器负责处理应用程序中的错误情况，并返回相应的错误视图
/// </remarks>
public class ErrorController : Controller
{
    /// <summary>
    /// 处理一般错误
    /// </summary>
    /// <returns>错误视图</returns>
    public IActionResult Error() => View();
}
