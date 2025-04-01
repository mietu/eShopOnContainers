namespace Microsoft.eShopOnContainers.WebMVC.ViewModels;

/// <summary>
/// 表示订单中的商品项目视图模型
/// </summary>
public record OrderItem
{
    /// <summary>
    /// 获取或初始化商品ID
    /// </summary>
    public int ProductId { get; init; }

    /// <summary>
    /// 获取或初始化商品名称
    /// </summary>
    public string ProductName { get; init; }

    /// <summary>
    /// 获取或初始化商品单价
    /// </summary>
    public decimal UnitPrice { get; init; }

    /// <summary>
    /// 获取或初始化商品折扣
    /// </summary>
    public decimal Discount { get; init; }

    /// <summary>
    /// 获取或初始化商品数量
    /// </summary>
    public int Units { get; init; }

    /// <summary>
    /// 获取或初始化商品图片URL
    /// </summary>
    public string PictureUrl { get; init; }
}
