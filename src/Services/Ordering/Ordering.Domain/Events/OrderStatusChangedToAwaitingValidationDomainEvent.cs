namespace Microsoft.eShopOnContainers.Services.Ordering.Domain.Events;

/// <summary>
/// 当宽限期订单确认时触发的事件
/// </summary>
public class OrderStatusChangedToAwaitingValidationDomainEvent : INotification
{
    // 订单标识符：当前事件关联的订单ID
    public int OrderId { get; }

    // 订单项集合：包含订单中的所有订单项
    public IEnumerable<OrderItem> OrderItems { get; }

    /// <summary>
    /// 构造函数：初始化事件时设置订单ID和订单项集合
    /// </summary>
    /// <param name="orderId">订单的唯一标识符</param>
    /// <param name="orderItems">订单中的所有订单项</param>
    public OrderStatusChangedToAwaitingValidationDomainEvent(int orderId,
        IEnumerable<OrderItem> orderItems)
    {
        // 给OrderId属性赋值，表示当前订单的ID
        OrderId = orderId;

        // 给OrderItems属性赋值，表示该订单包含的所有订单项
        OrderItems = orderItems;
    }
}
