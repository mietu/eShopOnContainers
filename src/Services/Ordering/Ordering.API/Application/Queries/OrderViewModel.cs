namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.Queries;

// 订单项记录，描述订单中某个商品的信息
public record Orderitem
{
    // 商品名称
    public string productname { get; init; }
    // 商品数量
    public int units { get; init; }
    // 商品单价
    public double unitprice { get; init; }
    // 商品图片链接
    public string pictureurl { get; init; }
}

// 完整订单记录，包含订单的基础信息及订单项集合
public record Order
{
    // 订单编号
    public int ordernumber { get; init; }
    // 订单创建日期
    public DateTime date { get; init; }
    // 订单当前状态
    public string status { get; init; }
    // 订单描述信息
    public string description { get; init; }
    // 送货地址：街道信息
    public string street { get; init; }
    // 城市信息
    public string city { get; init; }
    // 邮编信息
    public string zipcode { get; init; }
    // 国家信息
    public string country { get; init; }
    // 包含的订单项列表
    public List<Orderitem> orderitems { get; set; }
    // 订单总金额
    public decimal total { get; set; }
}

// 订单摘要记录，只包含部分订单的信息，用于展示
public record OrderSummary
{
    // 订单编号
    public int ordernumber { get; init; }
    // 订单创建日期
    public DateTime date { get; init; }
    // 订单当前状态
    public string status { get; init; }
    // 订单总金额（以双精度浮点数表示）
    public double total { get; init; }
}

// 卡类型记录，描述一种支付卡的类型
public record CardType
{
    // 卡类型唯一标识符
    public int Id { get; init; }
    // 卡类型名称
    public string Name { get; init; }
}
