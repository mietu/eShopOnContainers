namespace Microsoft.eShopOnContainers.WebMVC.ViewModels;

/// <summary>
/// 表示用户的购物篮。
/// 包含购物篮中的商品列表和买家标识信息，并提供计算总价的功能。
/// </summary>
public record Basket
{
    // 使用属性初始化器语法。
    // 这种语法虽然对只读的自动实现属性更为有用，
    // 但对读写属性也能简化逻辑。

    /// <summary>
    /// 获取或初始化购物篮中的商品项列表。
    /// 默认初始化为空列表。
    /// </summary>
    public List<BasketItem> Items { get; init; } = new List<BasketItem>();

    /// <summary>
    /// 获取或初始化购物篮所属买家的唯一标识符。
    /// </summary>
    public string BuyerId { get; init; }

    /// <summary>
    /// 计算购物篮中所有商品的总价。
    /// 总价等于每个商品单价乘以数量的总和，结果四舍五入到2位小数。
    /// </summary>
    /// <returns>购物篮中所有商品的总价值。</returns>
    public decimal Total()
    {
        return Math.Round(Items.Sum(x => x.UnitPrice * x.Quantity), 2);
    }
}
