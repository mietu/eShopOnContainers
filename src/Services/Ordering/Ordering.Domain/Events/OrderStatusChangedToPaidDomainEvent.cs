namespace Microsoft.eShopOnContainers.Services.Ordering.Domain.Events;

/// <summary>
/// 当订单支付成功时触发的事件
/// 用于通知系统订单状态已变更为已支付
/// </summary>
public class OrderStatusChangedToPaidDomainEvent : INotification
{
    // 订单的唯一标识
    public int OrderId { get; }

    // 订单中包含的所有订单项
    public IEnumerable<OrderItem> OrderItems { get; }

    // 构造函数，初始化订单ID和订单项集合
    public OrderStatusChangedToPaidDomainEvent(int orderId, IEnumerable<OrderItem> orderItems)
    {
        OrderId = orderId;         // 将传入的订单ID赋值给属性
        OrderItems = orderItems;   // 将传入的订单项集合赋值给属性
    }
}
