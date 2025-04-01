namespace Microsoft.eShopOnContainers.Web.Shopping.HttpAggregator.Models;

/// <summary>
/// 此类用于封装更新购物篮（Basket）的请求数据。
/// </summary>
public class UpdateBasketRequest
{
    /// <summary>
    /// 购物者ID，用于标识请求的用户。
    /// </summary>
    public string BuyerId { get; set; }

    /// <summary>
    /// 包含多个更新项的数据集合，表示购物篮中各个商品的更新信息。
    /// </summary>
    public IEnumerable<UpdateBasketRequestItemData> Items { get; set; }
}
