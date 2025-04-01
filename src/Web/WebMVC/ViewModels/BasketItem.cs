namespace Microsoft.eShopOnContainers.WebMVC.ViewModels;

/// <summary>
/// 表示购物篮中的一个商品项
/// </summary>
public record BasketItem
{
    /// <summary>
    /// 购物篮项的唯一标识符
    /// </summary>
    public string Id { get; init; }

    /// <summary>
    /// 商品的唯一标识符
    /// </summary>
    public int ProductId { get; init; }

    /// <summary>
    /// 商品名称
    /// </summary>
    public string ProductName { get; init; }

    /// <summary>
    /// 商品当前单价
    /// </summary>
    public decimal UnitPrice { get; init; }

    /// <summary>
    /// 商品原始单价，用于显示折扣信息
    /// </summary>
    public decimal OldUnitPrice { get; init; }

    /// <summary>
    /// 购物篮中该商品的数量
    /// </summary>
    public int Quantity { get; init; }

    /// <summary>
    /// 商品图片URL地址
    /// </summary>
    public string PictureUrl { get; init; }
}
