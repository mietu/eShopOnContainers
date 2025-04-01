namespace Microsoft.eShopOnContainers.Web.Shopping.HttpAggregator.Models;

/// <summary>
/// 表示更新购物篮中商品数量的数据模型
/// </summary>
public class UpdateBasketItemData
{
    /// <summary>
    /// 获取或设置购物篮项目的标识符
    /// </summary>
    public string BasketItemId { get; set; }

    /// <summary>
    /// 获取或设置购物篮项目的新数量
    /// </summary>
    public int NewQty { get; set; }
}
