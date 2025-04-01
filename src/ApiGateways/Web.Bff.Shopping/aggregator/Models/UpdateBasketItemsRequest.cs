namespace Microsoft.eShopOnContainers.Web.Shopping.HttpAggregator.Models;

/// <summary>
/// 表示更新购物篮中所有商品数量的请求数据模型
/// </summary>
public class UpdateBasketItemsRequest
{
    /// <summary>
    /// 获取或设置要更新的购物篮标识符
    /// </summary>
    public string BasketId { get; set; }

    /// <summary>
    /// 获取或设置购物篮中各项目更新的数据集合
    /// </summary>
    public ICollection<UpdateBasketItemData> Updates { get; set; }

    /// <summary>
    /// 初始化 <see cref="UpdateBasketItemsRequest"/> 类的新实例
    /// </summary>
    public UpdateBasketItemsRequest()
    {
        // 初始化更新集合，避免出现空引用问题
        Updates = new List<UpdateBasketItemData>();
    }
}
