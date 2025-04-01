namespace Microsoft.eShopOnContainers.WebMVC.Services;
using Microsoft.eShopOnContainers.WebMVC.ViewModels;

/// <summary>
/// 订单服务接口，提供订单管理相关功能
/// </summary>
public interface IOrderingService
{
    /// <summary>
    /// 获取指定用户的所有订单
    /// </summary>
    /// <param name="user">应用程序用户</param>
    /// <returns>用户的订单列表</returns>
    Task<List<Order>> GetMyOrders(ApplicationUser user);

    /// <summary>
    /// 获取指定用户的特定订单详情
    /// </summary>
    /// <param name="user">应用程序用户</param>
    /// <param name="orderId">订单ID</param>
    /// <returns>订单详情</returns>
    Task<Order> GetOrder(ApplicationUser user, string orderId);

    /// <summary>
    /// 取消指定订单
    /// </summary>
    /// <param name="orderId">要取消的订单ID</param>
    /// <returns>表示异步操作的任务</returns>
    Task CancelOrder(string orderId);

    /// <summary>
    /// 设置指定订单为已发货状态
    /// </summary>
    /// <param name="orderId">要发货的订单ID</param>
    /// <returns>表示异步操作的任务</returns>
    Task ShipOrder(string orderId);

    /// <summary>
    /// 将用户信息映射到订单对象中
    /// </summary>
    /// <param name="user">应用程序用户</param>
    /// <param name="order">需要填充用户信息的订单</param>
    /// <returns>填充了用户信息的订单</returns>
    Order MapUserInfoIntoOrder(ApplicationUser user, Order order);

    /// <summary>
    /// 将订单信息映射为购物篮DTO
    /// </summary>
    /// <param name="order">源订单</param>
    /// <returns>包含订单信息的购物篮DTO</returns>
    BasketDTO MapOrderToBasket(Order order);

    /// <summary>
    /// 将源订单中的用户信息覆盖到目标订单中
    /// </summary>
    /// <param name="original">源订单</param>
    /// <param name="destination">目标订单</param>
    void OverrideUserInfoIntoOrder(Order original, Order destination);
}
