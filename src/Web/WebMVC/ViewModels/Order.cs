namespace Microsoft.eShopOnContainers.WebMVC.ViewModels;

/// <summary>
/// 表示电子商务系统中的订单信息视图模型
/// 包含订单基本信息、收货地址、支付信息和订单项
/// </summary>
public class Order
{
    /// <summary>
    /// 订单编号 - 使用自定义JSON转换器处理数字到字符串的转换
    /// </summary>
    [JsonConverter(typeof(NumberToStringConverter))]
    public string OrderNumber { get; set; }

    /// <summary>
    /// 订单创建日期
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// 订单当前状态 (如: paid, shipped等)
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    /// 订单总金额
    /// </summary>
    public decimal Total { get; set; }

    /// <summary>
    /// 订单描述信息
    /// </summary>
    public string Description { get; set; }

    // 收货地址信息
    /// <summary>
    /// 收货城市
    /// </summary>
    [Required]
    public string City { get; set; }

    /// <summary>
    /// 收货街道地址
    /// </summary>
    [Required]
    public string Street { get; set; }

    /// <summary>
    /// 收货州/省
    /// </summary>
    [Required]
    public string State { get; set; }

    /// <summary>
    /// 收货国家
    /// </summary>
    [Required]
    public string Country { get; set; }

    /// <summary>
    /// 邮政编码
    /// </summary>
    public string ZipCode { get; set; }

    // 支付信息
    /// <summary>
    /// 支付卡号
    /// </summary>
    [Required]
    [DisplayName("Card number")]
    public string CardNumber { get; set; }

    /// <summary>
    /// 持卡人姓名
    /// </summary>
    [Required]
    [DisplayName("Cardholder name")]
    public string CardHolderName { get; set; }

    /// <summary>
    /// 卡片到期日期 (DateTime格式)
    /// </summary>
    public DateTime CardExpiration { get; set; }

    /// <summary>
    /// 卡片到期日期 (MM/YY格式)
    /// 格式验证和过期验证
    /// </summary>
    [RegularExpression(@"(0[1-9]|1[0-2])\/[0-9]{2}", ErrorMessage = "Expiration should match a valid MM/YY value")]
    [CardExpiration(ErrorMessage = "The card is expired"), Required]
    [DisplayName("Card expiration")]
    public string CardExpirationShort { get; set; }

    /// <summary>
    /// 卡片安全码 (CVV)
    /// </summary>
    [Required]
    [DisplayName("Card security number")]
    public string CardSecurityNumber { get; set; }

    /// <summary>
    /// 卡片类型ID
    /// </summary>
    public int CardTypeId { get; set; }

    /// <summary>
    /// 买家ID或用户名
    /// </summary>
    public string Buyer { get; set; }

    /// <summary>
    /// 基于当前订单状态可执行的操作代码列表
    /// </summary>
    public List<SelectListItem> ActionCodeSelectList =>
        GetActionCodesByCurrentState();

    /// <summary>
    /// 订单中包含的商品项列表
    /// </summary>
    public List<OrderItem> OrderItems { get; set; }

    /// <summary>
    /// 请求ID，用于跟踪和标识订单请求
    /// </summary>
    [Required]
    public Guid RequestId { get; set; }

    /// <summary>
    /// 将CardExpiration转换为CardExpirationShort格式 (MM/yy)
    /// </summary>
    public void CardExpirationShortFormat()
    {
        CardExpirationShort = CardExpiration.ToString("MM/yy");
    }

    /// <summary>
    /// 将CardExpirationShort (MM/YY)转换为API所需的DateTime格式
    /// </summary>
    public void CardExpirationApiFormat()
    {
        var month = CardExpirationShort.Split('/')[0];
        var year = $"20{CardExpirationShort.Split('/')[1]}";

        CardExpiration = new DateTime(int.Parse(year), int.Parse(month), 1);
    }

    /// <summary>
    /// 根据订单当前状态获取可执行的操作代码列表
    /// </summary>
    /// <returns>可执行操作的下拉列表项</returns>
    private List<SelectListItem> GetActionCodesByCurrentState()
    {
        var actions = new List<OrderProcessAction>();
        switch (Status?.ToLower())
        {
            case "paid":
                actions.Add(OrderProcessAction.Ship);
                break;
        }

        var result = new List<SelectListItem>();
        actions.ForEach(action =>
        {
            result.Add(new SelectListItem { Text = action.Name, Value = action.Code });
        });

        return result;
    }
}

/// <summary>
/// 支付卡类型枚举
/// </summary>
public enum CardType
{
    /// <summary>
    /// 美国运通卡
    /// </summary>
    AMEX = 1
}
