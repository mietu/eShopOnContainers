namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.IntegrationEvents.Events;

// 定义一个不可变记录类型 UserCheckoutAcceptedIntegrationEvent
// 此事件封装了用户结账时的必要信息，并继承自 IntegrationEvent
public record UserCheckoutAcceptedIntegrationEvent : IntegrationEvent
{
    // 用户唯一标识符
    public string UserId { get; }

    // 用户的名称
    public string UserName { get; }

    // 用户所在城市
    public string City { get; set; }

    // 用户所在街道地址
    public string Street { get; set; }

    // 用户所在州/省
    public string State { get; set; }

    // 用户所在国家
    public string Country { get; set; }

    // 用户邮政编码
    public string ZipCode { get; set; }

    // 信用卡号
    public string CardNumber { get; set; }

    // 信用卡持有者名称
    public string CardHolderName { get; set; }

    // 信用卡过期日期
    public DateTime CardExpiration { get; set; }

    // 信用卡安全码
    public string CardSecurityNumber { get; set; }

    // 信用卡类型标识符
    public int CardTypeId { get; set; }

    // 购买者信息
    public string Buyer { get; set; }

    // 请求唯一标识符，用于跟踪请求
    public Guid RequestId { get; set; }

    // 顾客购物篮，含有所有待购买商品
    public CustomerBasket Basket { get; }

    // 构造函数初始化所有属性
    public UserCheckoutAcceptedIntegrationEvent(
        string userId,         // 用户ID
        string userName,       // 用户名称
        string city,           // 城市
        string street,         // 街道地址
        string state,          // 州/省
        string country,        // 国家
        string zipCode,        // 邮政编码
        string cardNumber,     // 信用卡号
        string cardHolderName, // 信用卡持有者名称
        DateTime cardExpiration,       // 信用卡过期日期
        string cardSecurityNumber,     // 信用卡安全码
        int cardTypeId,        // 信用卡类型ID
        string buyer,          // 购买者信息
        Guid requestId,        // 请求ID
        CustomerBasket basket  // 顾客购物篮
    )
    {
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
        RequestId = requestId;
        Basket = basket;
    }
}
