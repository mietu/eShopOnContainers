namespace Microsoft.eShopOnContainers.Mobile.Shopping.HttpAggregator.Models;

/// <summary>
/// 表示添加商品到购物篮的请求模型。
/// </summary>
public class AddBasketItemRequest
{
    /// <summary>
    /// 获取或设置商品目录的唯一标识符。
    /// </summary>
    public int CatalogItemId { get; set; }

    /// <summary>
    /// 获取或设置购物篮的唯一标识符。
    /// </summary>
    public string BasketId { get; set; }

    /// <summary>
    /// 获取或设置添加到购物篮的商品数量，默认值为1。
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// 构造函数，初始化商品数量为1。
    /// </summary>
    public AddBasketItemRequest()
    {
        Quantity = 1;
    }
}
