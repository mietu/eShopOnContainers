namespace Microsoft.eShopOnContainers.Mobile.Shopping.HttpAggregator.Models;

// 表示更新购物篮中多个商品项的请求数据模型
public class UpdateBasketItemsRequest
{
    // 购物篮的标识符，用于唯一确定某个购物篮
    public string BasketId { get; set; }

    // 商品项更新集合，每个元素的数据类型为 UpdateBasketItemData，表示单个商品项的更新信息
    public ICollection<UpdateBasketItemData> Updates { get; set; }

    // 构造函数，初始化更新集合为新的 List 实例，避免出现空引用异常
    public UpdateBasketItemsRequest()
    {
        Updates = new List<UpdateBasketItemData>();
    }
}
