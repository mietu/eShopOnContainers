namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.Commands;

// DDD and CQRS patterns comment: Note that it is recommended to implement immutable Commands
// In this case, its immutability is achieved by having all the setters as private
// plus only being able to update the data just once, when creating the object through its constructor.
// References on Immutable Commands:  
// http://cqrs.nu/Faq
// https://docs.spine3.org/motivation/immutability.html 
// http://blog.gauffin.org/2012/06/griffin-container-introducing-command-support/
// https://docs.microsoft.com/dotnet/csharp/programming-guide/classes-and-structs/how-to-implement-a-lightweight-class-with-auto-implemented-properties

using Microsoft.eShopOnContainers.Services.Ordering.API.Application.Models;

[DataContract]
public class CreateOrderCommand : IRequest<bool>
{
    // 私有只读集合，用于存储 OrderItemDTO 类型的订单项数据
    [DataMember]
    private readonly List<OrderItemDTO> _orderItems;

    // 各种订单相关的属性，使用 DataMember 标记以便序列化
    [DataMember]
    public string UserId { get; private set; }  // 用户标识

    [DataMember]
    public string UserName { get; private set; }  // 用户名

    [DataMember]
    public string City { get; private set; }  // 收货城市

    [DataMember]
    public string Street { get; private set; }  // 收货街道

    [DataMember]
    public string State { get; private set; }  // 收货州

    [DataMember]
    public string Country { get; private set; }  // 收货国家

    [DataMember]
    public string ZipCode { get; private set; }  // 收货邮编

    [DataMember]
    public string CardNumber { get; private set; }  // 信用卡号码

    [DataMember]
    public string CardHolderName { get; private set; }  // 持卡人姓名

    [DataMember]
    public DateTime CardExpiration { get; private set; }  // 信用卡到期日

    [DataMember]
    public string CardSecurityNumber { get; private set; }  // 信用卡安全码

    [DataMember]
    public int CardTypeId { get; private set; }  // 信用卡类型标识

    // 公开的只读属性，返回订单事项列表
    [DataMember]
    public IEnumerable<OrderItemDTO> OrderItems => _orderItems;

    // 默认构造函数，初始化订单项集合
    public CreateOrderCommand()
    {
        _orderItems = new List<OrderItemDTO>();
    }

    // 构造函数，接受购物篮项及各种订单相关参数，调用默认构造函数初始化集合
    public CreateOrderCommand(List<BasketItem> basketItems, string userId, string userName, string city, string street, string state, string country, string zipcode,
        string cardNumber, string cardHolderName, DateTime cardExpiration,
        string cardSecurityNumber, int cardTypeId) : this()
    {
        // 将 BasketItem 转换为 OrderItemDTO 对象并存入列表中
        _orderItems = basketItems.ToOrderItemsDTO().ToList();
        UserId = userId;                   // 设置用户标识
        UserName = userName;               // 设置用户名
        City = city;                       // 设置城市
        Street = street;                   // 设置街道
        State = state;                     // 设置州
        Country = country;                 // 设置国家
        ZipCode = zipcode;                 // 设置邮编
        CardNumber = cardNumber;           // 设置信用卡号码
        CardHolderName = cardHolderName;   // 设置持卡人姓名
        CardExpiration = cardExpiration;   // 设置信用卡到期日期
        CardSecurityNumber = cardSecurityNumber; // 设置信用卡安全码
        CardTypeId = cardTypeId;           // 设置信用卡类型标识
    }

    // 内部定义的记录类型，描述单个订单项的信息
    public record OrderItemDTO
    {
        // 产品唯一标识
        public int ProductId { get; init; }

        // 产品名称
        public string ProductName { get; init; }

        // 产品单价
        public decimal UnitPrice { get; init; }

        // 折扣金额
        public decimal Discount { get; init; }

        // 商品数量
        public int Units { get; init; }

        // 产品图片 URL
        public string PictureUrl { get; init; }
    }
}
