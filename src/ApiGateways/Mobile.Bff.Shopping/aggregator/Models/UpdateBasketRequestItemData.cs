namespace Microsoft.eShopOnContainers.Mobile.Shopping.HttpAggregator.Models;

/// <summary>
/// 表示更新购物篮中商品的请求数据。
/// </summary>
public class UpdateBasketRequestItemData
{
    /// <summary>
    /// 获取或设置购物篮的标识符。
    /// </summary>
    public string Id { get; set; }          // 购物篮ID

    /// <summary>
    /// 获取或设置要更新的商品的标识符（目录项ID）。
    /// </summary>
    public int ProductId { get; set; }      // 目录项ID

    /// <summary>
    /// 获取或设置商品的数量。
    /// </summary>
    public int Quantity { get; set; }       // 商品数量
}
