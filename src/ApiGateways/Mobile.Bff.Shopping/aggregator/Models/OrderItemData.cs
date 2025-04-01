namespace Microsoft.eShopOnContainers.Mobile.Shopping.HttpAggregator.Models;

/// <summary>
/// 表示订单中某项商品的数据。
/// </summary>
public class OrderItemData
{
    /// <summary>
    /// 获取或设置商品的唯一标识符。
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// 获取或设置商品名称。
    /// </summary>
    public string ProductName { get; set; }

    /// <summary>
    /// 获取或设置商品单价。
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// 获取或设置商品折扣金额。
    /// </summary>
    public decimal Discount { get; set; }

    /// <summary>
    /// 获取或设置购买的商品数量。
    /// </summary>
    public int Units { get; set; }

    /// <summary>
    /// 获取或设置商品图片的URL地址。
    /// </summary>
    public string PictureUrl { get; set; }
}
