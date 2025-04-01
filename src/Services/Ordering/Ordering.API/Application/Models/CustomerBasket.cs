namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.Models;

/// <summary>
/// 表示顾客的购物篮
/// </summary>
public class CustomerBasket
{
    /// <summary>
    /// 顾客的唯一标识符
    /// </summary>
    public string BuyerId { get; set; }

    /// <summary>
    /// 顾客购物篮中的商品列表
    /// </summary>
    public List<BasketItem> Items { get; set; }

    /// <summary>
    /// 构造函数，初始化顾客购物篮
    /// </summary>
    /// <param name="buyerId">顾客的唯一标识符</param>
    /// <param name="items">购物篮中的商品列表</param>
    public CustomerBasket(string buyerId, List<BasketItem> items)
    {
        // 分配顾客标识符
        BuyerId = buyerId;
        // 初始化购物篮中的商品列表
        Items = items;
    }
}
