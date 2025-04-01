namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.IntegrationEvents.Events;

/// <summary>
/// 表示订单状态变更为“已提交”的集成事件
/// 当一个订单的状态变更为已提交时，将发布该事件以便其他服务能够根据此事件作出响应
/// </summary>
public record OrderStatusChangedToSubmittedIntegrationEvent : IntegrationEvent
{
    // 订单的唯一标识
    public int OrderId { get; }

    // 订单的状态，如 "Submitted" 或其他状态描述
    public string OrderStatus { get; }

    // 买家的姓名，用以标识下单的用户
    public string BuyerName { get; }

    /// <summary>
    /// 构造函数，初始化 OrderStatusChangedToSubmittedIntegrationEvent 实例
    /// </summary>
    /// <param name="orderId">订单的唯一标识</param>
    /// <param name="orderStatus">订单的状态</param>
    /// <param name="buyerName">买家的姓名</param>
    public OrderStatusChangedToSubmittedIntegrationEvent(int orderId, string orderStatus, string buyerName)
    {
        OrderId = orderId;
        OrderStatus = orderStatus;
        BuyerName = buyerName;
    }
}
