namespace Microsoft.eShopOnContainers.Services.Ordering.Domain.Events;

// OrderCancelledDomainEvent 类实现了 INotification 接口，
// 该接口通常用于事件发布/订阅机制（如 MediatR 库）。
public class OrderCancelledDomainEvent : INotification
{
    // 属性：Order，表示被取消的订单实体
    public Order Order { get; }

    // 构造函数：传入被取消的订单以初始化领域事件实例
    public OrderCancelledDomainEvent(Order order)
    {
        // 将传入的订单赋值给 Order 属性
        Order = order;
    }
}

