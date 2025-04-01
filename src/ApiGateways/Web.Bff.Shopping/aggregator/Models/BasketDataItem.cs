namespace Microsoft.eShopOnContainers.Web.Shopping.HttpAggregator.Models;

/// <summary>
/// 表示购物篮中单个商品的数据模型。
/// </summary>
public class BasketDataItem
{
    /// <summary>
    /// 购物篮中该商品的唯一标识符。
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// 产品的唯一标识符。
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// 产品名称。
    /// </summary>
    public string ProductName { get; set; }

    /// <summary>
    /// 当前单价。
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// 原始单价（可能是折扣前的价格）。
    /// </summary>
    public decimal OldUnitPrice { get; set; }

    /// <summary>
    /// 该商品在购物篮中的购买数量。
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// 产品图片的URL地址。
    /// </summary>
    public string PictureUrl { get; set; }
}
