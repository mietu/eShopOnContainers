namespace Microsoft.eShopOnContainers.Mobile.Shopping.HttpAggregator.Models;

/// <summary>
/// 更新购物篮请求的数据模型
/// 代表一个更新购物篮中商品信息的请求。
/// </summary>
public class UpdateBasketRequest
{
    /// <summary>
    /// 购物者的唯一标识符
    /// 用于标识哪个用户的购物篮需要更新。
    /// </summary>
    public string BuyerId { get; set; }

    /// <summary>
    /// 待更新的购物篮中商品数据集合
    /// 包含每一个商品的更新信息，如商品ID和数量等。
    /// </summary>
    public IEnumerable<UpdateBasketRequestItemData> Items { get; set; }
}
