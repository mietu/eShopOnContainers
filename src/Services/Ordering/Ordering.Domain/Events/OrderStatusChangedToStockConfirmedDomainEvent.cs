namespace Microsoft.eShopOnContainers.Services.Ordering.Domain.Events;

/// <summary>
/// 当订单库存项目被确认时触发的事件
/// </summary>
public class OrderStatusChangedToStockConfirmedDomainEvent : INotification
{
    // 订单的唯一标识符，用于追踪触发事件的具体订单
    public int OrderId { get; }

    // 构造函数，初始化事件实例，将传入的订单编号赋值给只读属性 OrderId
    public OrderStatusChangedToStockConfirmedDomainEvent(int orderId)
        => OrderId = orderId;
}
