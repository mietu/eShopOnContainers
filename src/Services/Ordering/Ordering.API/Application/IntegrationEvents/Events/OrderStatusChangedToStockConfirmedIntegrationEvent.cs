namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.IntegrationEvents.Events;

// 记录类型，继承自 IntegrationEvent，用于传递订单状态变更为库存确认的事件数据。
public record OrderStatusChangedToStockConfirmedIntegrationEvent : IntegrationEvent
{
    // 订单的唯一标识
    public int OrderId { get; }

    // 当前订单的状态描述
    public string OrderStatus { get; }

    // 买家的名称
    public string BuyerName { get; }

    // 构造函数，初始化 OrderStatusChangedToStockConfirmedIntegrationEvent 实例
    // 参数 orderId: 订单ID
    // 参数 orderStatus: 订单状态
    // 参数 buyerName: 买家名称
    public OrderStatusChangedToStockConfirmedIntegrationEvent(int orderId, string orderStatus, string buyerName)
    {
        OrderId = orderId;
        OrderStatus = orderStatus;
        BuyerName = buyerName;
    }
}
