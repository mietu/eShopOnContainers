namespace Microsoft.eShopOnContainers.Web.Shopping.HttpAggregator.Models;

/// <summary>
/// 表示订单数据模型，包含订单的基本信息及订单项集合。
/// </summary>
public class OrderData
{
    // 订单编号
    public string OrderNumber { get; set; }

    // 订单日期
    public DateTime Date { get; set; }

    // 订单状态（例如：已支付、待发货等）
    public string Status { get; set; }

    // 订单总金额
    public decimal Total { get; set; }

    // 订单描述信息
    public string Description { get; set; }

    // 收货城市
    public string City { get; set; }

    // 收货街道
    public string Street { get; set; }

    // 收货州/省
    public string State { get; set; }

    // 收货国家
    public string Country { get; set; }

    // 收货邮编
    public string ZipCode { get; set; }

    // 信用卡号码
    public string CardNumber { get; set; }

    // 持卡人姓名
    public string CardHolderName { get; set; }

    // 是否为草稿订单
    public bool IsDraft { get; set; }

    // 信用卡到期日期（完整）
    public DateTime CardExpiration { get; set; }

    // 信用卡到期日期（简短格式）
    public string CardExpirationShort { get; set; }

    // 信用卡安全码
    public string CardSecurityNumber { get; set; }

    // 信用卡类型ID
    public int CardTypeId { get; set; }

    // 购买者名称或标识
    public string Buyer { get; set; }

    // 订单项集合，每个订单项包含产品信息等详细数据
    public List<OrderItemData> OrderItems { get; } = new();
}

