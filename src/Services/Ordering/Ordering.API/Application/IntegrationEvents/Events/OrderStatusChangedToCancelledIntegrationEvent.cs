namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.IntegrationEvents.Events;

// 继承自 IntegrationEvent 基类的记录，表示订单状态变为取消的集成事件
public record OrderStatusChangedToCancelledIntegrationEvent : IntegrationEvent
{
    // 订单的唯一标识
    public int OrderId { get; }
    // 订单的当前状态
    public string OrderStatus { get; }
    // 买家的名称
    public string BuyerName { get; }

    // 构造函数，初始化订单取消事件所需的属性
    public OrderStatusChangedToCancelledIntegrationEvent(int orderId, string orderStatus, string buyerName)
    {
        OrderId = orderId;
        OrderStatus = orderStatus;
        BuyerName = buyerName;
    }
}
