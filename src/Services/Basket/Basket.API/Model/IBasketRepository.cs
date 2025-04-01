namespace Microsoft.eShopOnContainers.Services.Basket.API.Model;

/// <summary>
/// 定义顾客购物篮仓储的接口
/// 包含获取、更新以及删除购物篮等操作
/// </summary>
public interface IBasketRepository
{
    /// <summary>
    /// 异步获取指定顾客的购物篮
    /// </summary>
    /// <param name="customerId">顾客的唯一标识符</param>
    /// <returns>返回顾客的购物篮对象</returns>
    Task<CustomerBasket> GetBasketAsync(string customerId);

    /// <summary>
    /// 获取所有拥有购物篮的用户标识列表
    /// </summary>
    /// <returns>返回包含所有用户标识的集合</returns>
    IEnumerable<string> GetUsers();

    /// <summary>
    /// 异步更新顾客的购物篮
    /// </summary>
    /// <param name="basket">包含更新信息的购物篮对象</param>
    /// <returns>返回更新后的购物篮对象</returns>
    Task<CustomerBasket> UpdateBasketAsync(CustomerBasket basket);

    /// <summary>
    /// 异步删除指定顾客的购物篮
    /// </summary>
    /// <param name="id">顾客的唯一标识符</param>
    /// <returns>返回一个布尔值表明操作是否成功</returns>
    Task<bool> DeleteBasketAsync(string id);
}

