namespace Microsoft.eShopOnContainers.Mobile.Shopping.HttpAggregator.Controllers;

// 设置路由前缀为空，表示该控制器的所有路由都是基于根路径开始的
[Route("")]
public class HomeController : Controller
{
    // GET 请求处理方法，对根路径的 GET 请求进行处理
    [HttpGet]
    public IActionResult Index()
    {
        // 创建一个重定向结果，重定向到项目的 Swagger 页面
        return new RedirectResult("~/swagger/index.html");
    }
}
