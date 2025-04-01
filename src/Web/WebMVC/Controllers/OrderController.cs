namespace Microsoft.eShopOnContainers.WebMVC.Controllers;

using Microsoft.eShopOnContainers.WebMVC.ViewModels;

/// <summary>
/// 订单控制器 - 负责处理订单相关的用户请求
/// 需要用户授权才能访问此控制器中的所有方法
/// </summary>
[Authorize]
public class OrderController : Controller
{
    private IOrderingService _orderSvc;
    private IBasketService _basketSvc;
    private readonly IIdentityParser<ApplicationUser> _appUserParser;

    /// <summary>
    /// 构造函数 - 通过依赖注入获取所需的服务
    /// </summary>
    /// <param name="orderSvc">订单服务</param>
    /// <param name="basketSvc">购物篮服务</param>
    /// <param name="appUserParser">用户身份解析器</param>
    public OrderController(IOrderingService orderSvc, IBasketService basketSvc, IIdentityParser<ApplicationUser> appUserParser)
    {
        _appUserParser = appUserParser;
        _orderSvc = orderSvc;
        _basketSvc = basketSvc;
    }

    /// <summary>
    /// 创建订单页面 - 显示订单草稿，用户可以在此确认订单信息
    /// </summary>
    /// <returns>返回包含订单草稿信息的视图</returns>
    public async Task<IActionResult> Create()
    {
        // 获取当前登录用户信息
        var user = _appUserParser.Parse(HttpContext.User);
        // 获取基于用户购物篮的订单草稿
        var order = await _basketSvc.GetOrderDraft(user.Id);
        // 将用户信息映射到订单中
        var vm = _orderSvc.MapUserInfoIntoOrder(user, order);
        // 格式化信用卡过期日期为短格式显示
        vm.CardExpirationShortFormat();

        return View(vm);
    }

    /// <summary>
    /// 提交订单 - 处理用户提交的订单信息，完成结账流程
    /// </summary>
    /// <param name="model">订单模型，包含用户提交的订单信息</param>
    /// <returns>成功时重定向到订单列表，失败时返回创建订单页面</returns>
    [HttpPost]
    public async Task<IActionResult> Checkout(Order model)
    {
        try
        {
            if (ModelState.IsValid)
            {
                // 获取当前登录用户信息
                var user = _appUserParser.Parse(HttpContext.User);
                // 将订单信息映射为购物篮对象
                var basket = _orderSvc.MapOrderToBasket(model);

                // 提交购物篮进行结账
                await _basketSvc.Checkout(basket);

                // 结账成功，重定向到订单历史列表
                return RedirectToAction("Index");
            }
        }
        catch (Exception ex)
        {
            // 捕获并显示订单创建过程中的错误
            ModelState.AddModelError("Error", $"无法创建新订单，请稍后再试 ({ex.GetType().Name} - {ex.Message})");
        }

        // 如果模型验证失败或发生异常，返回创建订单视图并显示错误信息
        return View("Create", model);
    }

    /// <summary>
    /// 取消订单 - 允许用户取消已下单但尚未处理的订单
    /// </summary>
    /// <param name="orderId">要取消的订单ID</param>
    /// <returns>重定向到订单列表页面</returns>
    public async Task<IActionResult> Cancel(string orderId)
    {
        // 调用订单服务取消指定订单
        await _orderSvc.CancelOrder(orderId);

        // 取消后重定向到订单历史列表
        return RedirectToAction("Index");
    }

    /// <summary>
    /// 订单详情 - 显示特定订单的详细信息
    /// </summary>
    /// <param name="orderId">要查看的订单ID</param>
    /// <returns>包含订单详情的视图</returns>
    public async Task<IActionResult> Detail(string orderId)
    {
        // 获取当前登录用户信息
        var user = _appUserParser.Parse(HttpContext.User);

        // 获取指定订单的详细信息
        var order = await _orderSvc.GetOrder(user, orderId);
        return View(order);
    }

    /// <summary>
    /// 订单列表 - 显示当前用户的所有订单历史
    /// </summary>
    /// <param name="item">订单过滤参数（如有）</param>
    /// <returns>包含用户订单列表的视图</returns>
    public async Task<IActionResult> Index(Order item)
    {
        // 获取当前登录用户信息
        var user = _appUserParser.Parse(HttpContext.User);
        // 获取用户的所有订单
        var vm = await _orderSvc.GetMyOrders(user);
        return View(vm);
    }
}
