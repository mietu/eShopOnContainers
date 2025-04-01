namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.IntegrationEvents.Events;

// 继承自 IntegrationEvent，用于表明订单状态变更为已发货的事件
public record OrderStatusChangedToShippedIntegrationEvent : IntegrationEvent
{
    // 订单编号
    public int OrderId { get; }

    // 订单状态文本描述
    public string OrderStatus { get; }

    // 购买者名称
    public string BuyerName { get; }

    // 构造函数，用于初始化所有属性
    public OrderStatusChangedToShippedIntegrationEvent(int orderId, string orderStatus, string buyerName)
    {
        OrderId = orderId;         // 设置订单编号
        OrderStatus = orderStatus; // 设置订单状态
        BuyerName = buyerName;     // 设置购买者名称
    }
}
