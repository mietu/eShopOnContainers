namespace Microsoft.eShopOnContainers.Services.Ordering.Domain.Events;

// 此域事件用于标识买家及其支付方式已验证，可以继续下单流程
public class BuyerAndPaymentMethodVerifiedDomainEvent : INotification
{
    // 买家信息
    public Buyer Buyer { get; private set; }

    // 支付方式信息
    public PaymentMethod Payment { get; private set; }

    // 关联的订单标识
    public int OrderId { get; private set; }

    // 构造函数：初始化买家、支付方式以及订单标识
    public BuyerAndPaymentMethodVerifiedDomainEvent(Buyer buyer, PaymentMethod payment, int orderId)
    {
        Buyer = buyer;
        Payment = payment;
        OrderId = orderId;
    }
}
