namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.Models;

// 表示购物篮中单个商品的模型
public class BasketItem
{
    // 商品篮项的唯一标识
    public string Id { get; init; }

    // 产品的唯一标识
    public int ProductId { get; init; }

    // 产品名称
    public string ProductName { get; init; }

    // 当前产品的单价
    public decimal UnitPrice { get; init; }

    // 产品之前的单价（可能用于对比或展示折扣前的价格）
    public decimal OldUnitPrice { get; init; }

    // 购物篮中该产品的数量
    public int Quantity { get; init; }

    // 产品图片的URL地址
    public string PictureUrl { get; init; }
}

