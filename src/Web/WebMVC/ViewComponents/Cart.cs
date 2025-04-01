namespace Microsoft.eShopOnContainers.WebMVC.ViewComponents;

/// <summary>
/// 购物车视图组件，用于在页面上显示用户购物车中的商品数量
/// </summary>
public class Cart : ViewComponent
{
    private readonly IBasketService _cartSvc;

    /// <summary>
    /// 构造函数，通过依赖注入接收购物篮服务
    /// </summary>
    /// <param name="cartSvc">购物篮服务接口</param>
    public Cart(IBasketService cartSvc) => _cartSvc = cartSvc;

    /// <summary>
    /// 视图组件的入口方法，异步获取并返回用户购物车信息
    /// </summary>
    /// <param name="user">当前应用用户</param>
    /// <returns>包含购物车信息的视图</returns>
    public async Task<IViewComponentResult> InvokeAsync(ApplicationUser user)
    {
        var vm = new CartComponentViewModel();
        try
        {
            // 获取用户购物车中的商品数量
            var itemsInCart = await ItemsInCartAsync(user);
            vm.ItemsCount = itemsInCart;
            return View(vm);
        }
        catch
        {
            // 当购物篮服务不可用时，设置状态标志
            ViewBag.IsBasketInoperative = true;
        }

        return View(vm);
    }

    /// <summary>
    /// 获取用户购物车中的商品数量
    /// </summary>
    /// <param name="user">当前应用用户</param>
    /// <returns>购物车中的商品数量</returns>
    private async Task<int> ItemsInCartAsync(ApplicationUser user)
    {
        var basket = await _cartSvc.GetBasket(user);
        return basket.Items.Count;
    }
}
