namespace Microsoft.eShopOnContainers.Services.Basket.API.Controllers;

/// <summary>
/// 首页控制器
/// 负责处理根路径请求并重定向到 Swagger 文档页面
/// </summary>
public class HomeController : Controller
{
    /// <summary>
    /// 处理对网站根目录的 GET 请求
    /// </summary>
    /// <returns>重定向到 Swagger UI 界面的结果</returns>
    public IActionResult Index()
    {
        return new RedirectResult("~/swagger/index.html");
    }
}

