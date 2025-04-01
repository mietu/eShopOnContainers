namespace Microsoft.eShopOnContainers.Services.Ordering.Domain.Events;

// 表示订单发货的领域事件，通知系统某个订单已被标记为发货状态
// 此事件实现了 INotification 接口，便于与 MediatR 等库进行集成
public class OrderShippedDomainEvent : INotification
{
    // 订单属性，保存与发货相关的订单信息
    public Order Order { get; }

    // 构造函数初始化领域事件，传入被发货的订单实例
    public OrderShippedDomainEvent(Order order)
    {
        Order = order;
    }
}
