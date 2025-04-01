namespace Microsoft.eShopOnContainers.WebMVC.Services;

using Microsoft.eShopOnContainers.WebMVC.ViewModels;

/// <summary>
/// 购物篮服务接口，提供与用户购物篮相关的操作
/// </summary>
public interface IBasketService
{
    /// <summary>
    /// 获取指定用户的购物篮
    /// </summary>
    /// <param name="user">应用程序用户</param>
    /// <returns>用户的购物篮</returns>
    Task<Basket> GetBasket(ApplicationUser user);

    /// <summary>
    /// 向用户的购物篮添加商品
    /// </summary>
    /// <param name="user">应用程序用户</param>
    /// <param name="productId">要添加的商品ID</param>
    /// <returns>异步任务</returns>
    Task AddItemToBasket(ApplicationUser user, int productId);

    /// <summary>
    /// 更新购物篮内容
    /// </summary>
    /// <param name="basket">更新后的购物篮</param>
    /// <returns>更新后的购物篮</returns>
    Task<Basket> UpdateBasket(Basket basket);

    /// <summary>
    /// 执行结账操作
    /// </summary>
    /// <param name="basket">要结账的购物篮数据</param>
    /// <returns>异步任务</returns>
    Task Checkout(BasketDTO basket);

    /// <summary>
    /// 设置购物篮中商品的数量
    /// </summary>
    /// <param name="user">应用程序用户</param>
    /// <param name="quantities">商品ID与数量的映射字典</param>
    /// <returns>更新后的购物篮</returns>
    Task<Basket> SetQuantities(ApplicationUser user, Dictionary<string, int> quantities);

    /// <summary>
    /// 获取基于购物篮的订单草稿
    /// </summary>
    /// <param name="basketId">购物篮ID</param>
    /// <returns>订单草稿</returns>
    Task<Order> GetOrderDraft(string basketId);
}
