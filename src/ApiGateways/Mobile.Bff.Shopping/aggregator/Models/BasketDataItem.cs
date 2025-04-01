namespace Microsoft.eShopOnContainers.Mobile.Shopping.HttpAggregator.Models;

/// <summary>
/// 表示购物篮中单个商品的数据模型
/// </summary>
public class BasketDataItem
{
    /// <summary>
    /// 商品的唯一标识符
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// 产品编号，用于标识具体产品
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// 产品名称，存储产品显示名称
    /// </summary>
    public string ProductName { get; set; }

    /// <summary>
    /// 当前单价，表示购买时的单价
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// 原始单价，可能用于显示折扣前的价格
    /// </summary>
    public decimal OldUnitPrice { get; set; }

    /// <summary>
    /// 购买数量，表示购物篮内该产品的数量
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// 产品图片的 URL 地址，用于显示产品图片
    /// </summary>
    public string PictureUrl { get; set; }
}
