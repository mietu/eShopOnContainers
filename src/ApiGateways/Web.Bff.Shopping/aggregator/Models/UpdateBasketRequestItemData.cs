namespace Microsoft.eShopOnContainers.Web.Shopping.HttpAggregator.Models;

// 此类用于表示更新购物篮中商品请求的数据
public class UpdateBasketRequestItemData
{
    // 代表购物篮的 Id，用于唯一标识购物篮
    public string Id { get; set; }

    // 表示商品的唯一标识符，即目录中商品的 Id
    public int ProductId { get; set; }

    // 代表要更新的商品数量
    public int Quantity { get; set; }
}
