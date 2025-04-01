namespace Microsoft.eShopOnContainers.WebMVC.ViewComponents;

/// <summary>
/// 购物篮列表视图组件
/// 用于展示当前用户的购物篮内容
/// </summary>
public class CartList : ViewComponent
{
    private readonly IBasketService _cartSvc;

    /// <summary>
    /// 构造函数，通过依赖注入获取购物篮服务
    /// </summary>
    /// <param name="cartSvc">购物篮服务</param>
    public CartList(IBasketService cartSvc) => _cartSvc = cartSvc;

    /// <summary>
    /// 视图组件调用方法，获取并展示用户的购物篮
    /// </summary>
    /// <param name="user">当前应用程序用户</param>
    /// <returns>包含购物篮信息的视图</returns>
    public async Task<IViewComponentResult> InvokeAsync(ApplicationUser user)
    {
        var vm = new Basket(); // 初始化一个空的购物篮作为视图模型
        try
        {
            vm = await GetItemsAsync(user); // 尝试获取用户的购物篮数据
            return View(vm); // 返回购物篮视图
        }
        catch (Exception ex)
        {
            // 当购物篮服务不可用时，设置错误消息
            ViewBag.BasketInoperativeMsg = $"Basket Service is inoperative, please try later on. ({ex.GetType().Name} - {ex.Message}))";
        }

        return View(vm); // 无论是否出现异常，都返回视图（可能是空购物篮或有错误信息）
    }

    /// <summary>
    /// 获取用户购物篮数据的私有辅助方法
    /// </summary>
    /// <param name="user">当前应用程序用户</param>
    /// <returns>用户的购物篮</returns>
    private Task<Basket> GetItemsAsync(ApplicationUser user) => _cartSvc.GetBasket(user);
}
