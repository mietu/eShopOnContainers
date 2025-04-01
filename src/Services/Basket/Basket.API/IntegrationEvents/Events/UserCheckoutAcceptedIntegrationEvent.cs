namespace Basket.API.IntegrationEvents.Events;

/// <summary>
/// 表示用户结账已接受的集成事件，用于跨服务传递用户结账时的相关详细信息
/// </summary>
public record UserCheckoutAcceptedIntegrationEvent : IntegrationEvent
{
    /// <summary>
    /// 用户的唯一标识符
    /// </summary>
    public string UserId { get; }

    /// <summary>
    /// 用户名称
    /// </summary>
    public string UserName { get; }

    /// <summary>
    /// 订单号，可在初始化后修改
    /// </summary>
    public int OrderNumber { get; init; }

    /// <summary>
    /// 城市
    /// </summary>
    public string City { get; init; }

    /// <summary>
    /// 街道地址
    /// </summary>
    public string Street { get; init; }

    /// <summary>
    /// 省份或州
    /// </summary>
    public string State { get; init; }

    /// <summary>
    /// 国家
    /// </summary>
    public string Country { get; init; }

    /// <summary>
    /// 邮政编码
    /// </summary>
    public string ZipCode { get; init; }

    /// <summary>
    /// 信用卡号码
    /// </summary>
    public string CardNumber { get; init; }

    /// <summary>
    /// 信用卡持有者姓名
    /// </summary>
    public string CardHolderName { get; init; }

    /// <summary>
    /// 信用卡到期日期
    /// </summary>
    public DateTime CardExpiration { get; init; }

    /// <summary>
    /// 信用卡安全码
    /// </summary>
    public string CardSecurityNumber { get; init; }

    /// <summary>
    /// 信用卡类型编号
    /// </summary>
    public int CardTypeId { get; init; }

    /// <summary>
    /// 买家名称
    /// </summary>
    public string Buyer { get; init; }

    /// <summary>
    /// 请求标识，用于跟踪和关联相关请求
    /// </summary>
    public Guid RequestId { get; init; }

    /// <summary>
    /// 顾客购物篮，包含购物篮中所有的商品信息
    /// </summary>
    public CustomerBasket Basket { get; }

    /// <summary>
    /// 构造函数：初始化所有必需的信息以创建一个新的用户结账已接受的集成事件实例
    /// </summary>
    /// <param name="userId">用户唯一标识</param>
    /// <param name="userName">用户名称</param>
    /// <param name="city">所在城市</param>
    /// <param name="street">街道地址</param>
    /// <param name="state">省份或州</param>
    /// <param name="country">国家</param>
    /// <param name="zipCode">邮政编码</param>
    /// <param name="cardNumber">信用卡号码</param>
    /// <param name="cardHolderName">信用卡持有者姓名</param>
    /// <param name="cardExpiration">信用卡到期时间</param>
    /// <param name="cardSecurityNumber">信用卡安全码</param>
    /// <param name="cardTypeId">信用卡类型标识</param>
    /// <param name="buyer">买家名称</param>
    /// <param name="requestId">请求标识符</param>
    /// <param name="basket">顾客购物篮</param>
    public UserCheckoutAcceptedIntegrationEvent(
        string userId,
        string userName,
        string city,
        string street,
        string state,
        string country,
        string zipCode,
        string cardNumber,
        string cardHolderName,
        DateTime cardExpiration,
        string cardSecurityNumber,
        int cardTypeId,
        string buyer,
        Guid requestId,
        CustomerBasket basket)
    {
        // 初始化每个属性，使这个事件携带所有必需的信息
        UserId = userId;
        UserName = userName;
        City = city;
        Street = street;
        State = state;
        Country = country;
        ZipCode = zipCode;
        CardNumber = cardNumber;
        CardHolderName = cardHolderName;
        CardExpiration = cardExpiration;
        CardSecurityNumber = cardSecurityNumber;
        CardTypeId = cardTypeId;
        Buyer = buyer;
        Basket = basket;
        RequestId = requestId;
    }
}
