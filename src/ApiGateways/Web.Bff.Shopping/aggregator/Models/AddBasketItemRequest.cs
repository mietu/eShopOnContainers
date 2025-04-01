namespace Microsoft.eShopOnContainers.Web.Shopping.HttpAggregator.Models;

/// <summary>
/// 用于在篮子中添加商品的请求模型
/// </summary>
public class AddBasketItemRequest
{
    /// <summary>
    /// 商品目录项的ID
    /// </summary>
    public int CatalogItemId { get; set; }

    /// <summary>
    /// 篮子的ID
    /// </summary>
    public string BasketId { get; set; }

    /// <summary>
    /// 添加的商品数量，默认值为1
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// 构造函数，初始化Quantity属性为1
    /// </summary>
    public AddBasketItemRequest()
    {
        Quantity = 1;
    }
}

