namespace Microsoft.eShopOnContainers.WebMVC.Controllers;

/// <summary>
/// 购物车控制器，负责处理用户购物车相关的操作
/// 包括查看购物车、更新商品数量、添加商品到购物车等功能
/// </summary>
[Authorize] // 需要用户认证才能访问购物车功能
public class CartController : Controller
{
    private readonly IBasketService _basketSvc; // 购物篮服务，处理购物车数据操作
    private readonly ICatalogService _catalogSvc; // 商品目录服务，获取商品信息
    private readonly IIdentityParser<ApplicationUser> _appUserParser; // 用户身份解析器

    /// <summary>
    /// 构造函数，通过依赖注入获取所需服务
    /// </summary>
    /// <param name="basketSvc">购物篮服务</param>
    /// <param name="catalogSvc">商品目录服务</param>
    /// <param name="appUserParser">用户身份解析器</param>
    public CartController(IBasketService basketSvc, ICatalogService catalogSvc, IIdentityParser<ApplicationUser> appUserParser)
    {
        _basketSvc = basketSvc;
        _catalogSvc = catalogSvc;
        _appUserParser = appUserParser;
    }

    /// <summary>
    /// 显示当前用户的购物车内容
    /// </summary>
    /// <returns>包含购物车数据的视图</returns>
    public async Task<IActionResult> Index()
    {
        try
        {
            var user = _appUserParser.Parse(HttpContext.User); // 获取当前登录用户
            var vm = await _basketSvc.GetBasket(user); // 获取用户的购物篮数据

            return View(vm); // 返回视图，显示购物车内容
        }
        catch (Exception ex)
        {
            HandleException(ex); // 处理异常
        }

        return View(); // 发生异常时返回空的视图
    }

    /// <summary>
    /// 处理购物车表单提交，更新商品数量或进行结账
    /// </summary>
    /// <param name="quantities">商品ID和对应数量的字典</param>
    /// <param name="action">用户执行的操作，如结账</param>
    /// <returns>根据操作返回相应的视图或重定向</returns>
    [HttpPost]
    public async Task<IActionResult> Index(Dictionary<string, int> quantities, string action)
    {
        try
        {
            var user = _appUserParser.Parse(HttpContext.User); // 获取当前登录用户
            var basket = await _basketSvc.SetQuantities(user, quantities); // 更新购物篮中商品数量
            if (action == "[ Checkout ]") // 如果用户点击了结账按钮
            {
                return RedirectToAction("Create", "Order"); // 重定向到订单创建页面
            }
        }
        catch (Exception ex)
        {
            HandleException(ex); // 处理异常
        }

        return View(); // 更新数量后返回购物车视图
    }

    /// <summary>
    /// 将商品添加到购物车
    /// </summary>
    /// <param name="productDetails">要添加的商品详情</param>
    /// <returns>重定向到商品目录页面</returns>
    public async Task<IActionResult> AddToCart(CatalogItem productDetails)
    {
        try
        {
            if (productDetails?.Id != null) // 检查商品ID是否有效
            {
                var user = _appUserParser.Parse(HttpContext.User); // 获取当前登录用户
                await _basketSvc.AddItemToBasket(user, productDetails.Id); // 将商品添加到用户的购物篮
            }
            return RedirectToAction("Index", "Catalog"); // 返回商品目录页面
        }
        catch (Exception ex)
        {
            // 捕获购物篮服务不可用时的异常（如熔断模式）               
            HandleException(ex); // 处理异常
        }

        return RedirectToAction("Index", "Catalog", new { errorMsg = ViewBag.BasketInoperativeMsg }); // 发生异常时返回目录页面并显示错误信息
    }

    /// <summary>
    /// 处理购物车操作过程中的异常
    /// </summary>
    /// <param name="ex">捕获的异常</param>
    private void HandleException(Exception ex)
    {
        ViewBag.BasketInoperativeMsg = $"Basket Service is inoperative {ex.GetType().Name} - {ex.Message}"; // 设置错误消息
    }
}
