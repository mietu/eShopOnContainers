namespace WebStatus.Controllers;

/// <summary>
/// WebStatus应用的主控制器，处理主页、配置和错误页面的请求
/// </summary>
public class HomeController : Controller
{
    // 存储应用程序配置的私有字段
    private readonly IConfiguration _configuration;

    /// <summary>
    /// 构造函数，通过依赖注入接收应用程序配置
    /// </summary>
    /// <param name="configuration">应用程序配置接口</param>
    public HomeController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// 处理主页请求，重定向到健康检查UI页面
    /// </summary>
    /// <returns>重定向到健康检查UI的结果</returns>
    public IActionResult Index()
    {
        // 从配置中获取基础路径
        var basePath = _configuration["PATH_BASE"];
        // 重定向到健康检查UI页面
        return Redirect($"{basePath}/hc-ui");
    }

    /// <summary>
    /// 处理配置页面请求，显示所有健康检查相关的配置项
    /// </summary>
    /// <returns>包含配置值的视图</returns>
    [HttpGet("/Config")]
    public IActionResult Config()
    {
        // 收集两种可能格式的健康检查配置（注意两种不同的节点名称格式）
        var configurationValues = _configuration.GetSection("HealthChecksUI:HealthChecks")
            .GetChildren()
            .SelectMany(cs => cs.GetChildren())
            .Union(_configuration.GetSection("HealthChecks-UI:HealthChecks")
            .GetChildren()
            .SelectMany(cs => cs.GetChildren()))
            // 转换为字典，键为配置路径，值为配置值
            .ToDictionary(v => v.Path, v => v.Value);

        // 返回包含配置值的视图
        return View(configurationValues);
    }

    /// <summary>
    /// 处理错误页面请求
    /// </summary>
    /// <returns>错误视图</returns>
    public IActionResult Error()
    {
        return View();
    }
}
