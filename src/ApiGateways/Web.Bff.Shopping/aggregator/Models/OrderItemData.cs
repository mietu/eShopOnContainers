namespace Microsoft.eShopOnContainers.Web.Shopping.HttpAggregator.Models;

// OrderItemData 类表示订单项中的产品详细信息
public class OrderItemData
{
    // 产品唯一标识符，用于标识每个产品
    public int ProductId { get; set; }

    // 产品名称
    public string ProductName { get; set; }

    // 单位价格，代表单个产品的价格
    public decimal UnitPrice { get; set; }

    // 折扣，表示在订单中应用于此产品的折扣金额
    public decimal Discount { get; set; }

    // 数量，表示此产品在订单中购买的数量
    public int Units { get; set; }

    // 产品图片的 URL 地址，指向产品图片资源
    public string PictureUrl { get; set; }
}
