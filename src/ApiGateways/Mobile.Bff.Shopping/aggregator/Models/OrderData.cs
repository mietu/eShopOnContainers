namespace Microsoft.eShopOnContainers.Mobile.Shopping.HttpAggregator.Models;

/// <summary>
/// 表示订单的详细数据，包括订单基本信息和其项数据。
/// </summary>
public class OrderData
{
    // 订单号（例如：订单唯一标识符）
    public string OrderNumber { get; set; }

    // 订单的日期和时间
    public DateTime Date { get; set; }

    // 订单的状态（例如：待处理、完成、取消等）
    public string Status { get; set; }

    // 订单的总金额
    public decimal Total { get; set; }

    // 订单的描述信息
    public string Description { get; set; }

    // 订单配送目标城市
    public string City { get; set; }

    // 订单配送目标街道
    public string Street { get; set; }

    // 订单配送目标州或省份
    public string State { get; set; }

    // 订单配送目标国家
    public string Country { get; set; }

    // 订单配送目标邮编
    public string ZipCode { get; set; }

    // 支付卡号（敏感信息，谨慎处理）
    public string CardNumber { get; set; }

    // 持卡人名称
    public string CardHolderName { get; set; }

    // 标识订单是否处于草稿状态
    public bool IsDraft { get; set; }

    // 卡片的有效日期（完整日期格式）
    public DateTime CardExpiration { get; set; }

    // 卡片有效日期的简短字符串表示（例如 MM/yy 格式）
    public string CardExpirationShort { get; set; }

    // 支付卡的安全码（CVV/CVC等）
    public string CardSecurityNumber { get; set; }

    // 卡片类型标识符（例如：Visa, MasterCard 等）
    public int CardTypeId { get; set; }

    // 购买者的标识或名称
    public string Buyer { get; set; }

    // 订单中的各个商品项列表
    public List<OrderItemData> OrderItems { get; } = new();
}
