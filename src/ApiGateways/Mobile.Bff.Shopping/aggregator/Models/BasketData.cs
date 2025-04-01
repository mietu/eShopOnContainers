namespace Microsoft.eShopOnContainers.Mobile.Shopping.HttpAggregator.Models;

/// <summary>
/// 表示购物篮的数据模型，其中包含购物者标识和购物篮中的商品列表。
/// </summary>
public class BasketData
{
    /// <summary>
    /// 获取或设置购物者的标识。
    /// </summary>
    public string BuyerId { get; set; }

    /// <summary>
    /// 获取或设置购物篮中包含的商品列表。
    /// </summary>
    public List<BasketDataItem> Items { get; set; } = new();

    /// <summary>
    /// 无参数构造函数。
    /// </summary>
    public BasketData()
    {
    }

    /// <summary>
    /// 根据购物者标识初始化购物篮数据模型的构造函数。
    /// </summary>
    /// <param name="buyerId">购物者的标识</param>
    public BasketData(string buyerId)
    {
        BuyerId = buyerId;
    }
}
