namespace Microsoft.eShopOnContainers.Mobile.Shopping.HttpAggregator.Models;

/// <summary>
/// 表示用于更新购物篮中商品数量的数据模型
/// </summary>
public class UpdateBasketItemData
{
    /// <summary>
    /// 获取或设置购物篮项的唯一标识符
    /// 用于标识需要更新的购物篮中的具体商品
    /// </summary>
    public string BasketItemId { get; set; }

    /// <summary>
    /// 获取或设置商品的新数量
    /// 在更新购物篮时指定的新的商品数量
    /// </summary>
    public int NewQty { get; set; }

    /// <summary>
    /// 构造函数，初始化 NewQty 为默认值 0
    /// 表示默认情况下购物篮中商品的数量为 0
    /// </summary>
    public UpdateBasketItemData()
    {
        NewQty = 0;
    }
}
