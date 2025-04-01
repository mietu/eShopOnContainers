namespace WebMVC.Controllers;

/// <summary>
/// 订单管理控制器，负责处理用户订单的查看和处理
/// 此控制器要求用户必须登录才能访问
/// </summary>
[Authorize]
public class OrderManagementController : Controller
{
    private IOrderingService _orderSvc;
    private readonly IIdentityParser<ApplicationUser> _appUserParser;

    /// <summary>
    /// 构造函数，通过依赖注入获取所需服务
    /// </summary>
    /// <param name="orderSvc">订单服务接口，用于处理订单相关业务逻辑</param>
    /// <param name="appUserParser">用户身份解析器，用于从HttpContext中获取用户信息</param>
    public OrderManagementController(IOrderingService orderSvc, IIdentityParser<ApplicationUser> appUserParser)
    {
        _appUserParser = appUserParser;
        _orderSvc = orderSvc;
    }

    /// <summary>
    /// 获取并显示当前登录用户的所有订单
    /// </summary>
    /// <returns>包含用户订单列表的视图</returns>
    public async Task<IActionResult> Index()
    {
        // 从HttpContext.User中解析出当前登录用户
        var user = _appUserParser.Parse(HttpContext.User);
        // 通过订单服务获取该用户的所有订单
        var vm = await _orderSvc.GetMyOrders(user);

        // 返回包含订单列表的视图
        return View(vm);
    }

    /// <summary>
    /// 处理订单操作的POST请求
    /// 目前支持的操作：发货(Ship)
    /// </summary>
    /// <param name="orderId">要处理的订单ID</param>
    /// <param name="actionCode">操作代码，表示要执行的操作类型</param>
    /// <returns>重定向到订单列表页面</returns>
    [HttpPost]
    public async Task<IActionResult> OrderProcess(string orderId, string actionCode)
    {
        // 判断操作类型是否为"发货"
        if (OrderProcessAction.Ship.Code == actionCode)
        {
            // 调用订单服务执行发货操作
            await _orderSvc.ShipOrder(orderId);
        }

        // 操作完成后重定向回订单列表页面
        return RedirectToAction("Index");
    }
}
