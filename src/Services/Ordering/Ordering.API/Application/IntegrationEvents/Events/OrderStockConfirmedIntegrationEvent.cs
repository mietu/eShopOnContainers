namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.IntegrationEvents.Events;

// 继承自 IntegrationEvent 的记录类型，确保事件具有通用事件属性（如 Id 和 CreationDate）
public record OrderStockConfirmedIntegrationEvent : IntegrationEvent
{
    // OrderId 属性表示订单的唯一标识符
    public int OrderId { get; }

    // 构造函数：初始化 OrderStockConfirmedIntegrationEvent 实例时，必须提供订单ID
    public OrderStockConfirmedIntegrationEvent(int orderId) => OrderId = orderId;
}
