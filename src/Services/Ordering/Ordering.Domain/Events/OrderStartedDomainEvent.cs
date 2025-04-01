
namespace Microsoft.eShopOnContainers.Services.Ordering.Domain.Events
{
    /// <summary>
    /// 当一个订单被创建时触发的领域事件
    /// 该事件继承自 INotification，用于通知系统中的其他部分订单已经开始处理
    /// </summary>
    public class OrderStartedDomainEvent : INotification
    {
        // 用户 ID
        public string UserId { get; }
        // 用户名称
        public string UserName { get; }
        // 卡类型 ID（可能用于区分不同的付款方式，例如信用卡/借记卡）
        public int CardTypeId { get; }
        // 卡号
        public string CardNumber { get; }
        // 卡安全码，一般用于验证卡片
        public string CardSecurityNumber { get; }
        // 持卡人姓名
        public string CardHolderName { get; }
        // 卡失效日期
        public DateTime CardExpiration { get; }
        // 订单实体对象，包含订单详细信息
        public Order Order { get; }

        /// <summary>
        /// 构造函数，用于初始化 OrderStartedDomainEvent 的所有属性
        /// 当创建此事件时，必须提供相关的订单信息及付款信息
        /// </summary>
        /// <param name="order">订单实体</param>
        /// <param name="userId">用户 ID</param>
        /// <param name="userName">用户名称</param>
        /// <param name="cardTypeId">卡类型 ID</param>
        /// <param name="cardNumber">卡号</param>
        /// <param name="cardSecurityNumber">卡安全码</param>
        /// <param name="cardHolderName">持卡人姓名</param>
        /// <param name="cardExpiration">卡失效日期</param>
        public OrderStartedDomainEvent(Order order, string userId, string userName,
                                       int cardTypeId, string cardNumber,
                                       string cardSecurityNumber, string cardHolderName,
                                       DateTime cardExpiration)
        {
            // 将传入参数赋值给属性
            Order = order;
            UserId = userId;
            UserName = userName;
            CardTypeId = cardTypeId;
            CardNumber = cardNumber;
            CardSecurityNumber = cardSecurityNumber;
            CardHolderName = cardHolderName;
            CardExpiration = cardExpiration;
        }
    }
}
