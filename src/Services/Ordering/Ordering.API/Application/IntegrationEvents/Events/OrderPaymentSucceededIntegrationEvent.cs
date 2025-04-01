namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.IntegrationEvents.Events;

// 此记录类型继承自 IntegrationEvent，用于表示订单支付成功的集成事件
public record OrderPaymentSucceededIntegrationEvent : IntegrationEvent
{
    // 订单ID属性，仅可读
    public int OrderId { get; }

    // 构造函数，初始化订单支付成功事件，同时设置订单ID
    public OrderPaymentSucceededIntegrationEvent(int orderId) => OrderId = orderId;
}
