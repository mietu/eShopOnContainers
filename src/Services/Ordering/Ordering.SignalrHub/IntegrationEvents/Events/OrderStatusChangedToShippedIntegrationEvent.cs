namespace Microsoft.eShopOnContainers.Services.Ordering.SignalrHub.IntegrationEvents.Events;

/// <summary>
/// 订单状态变更为"已发货"的集成事件
/// 当订单状态更新为已发货时触发，用于跨服务通知和实时更新
/// </summary>
public record OrderStatusChangedToShippedIntegrationEvent : IntegrationEvent
{
    /// <summary>
    /// 订单的唯一标识
    /// </summary>
    public int OrderId { get; }

    /// <summary>
    /// 订单的当前状态
    /// </summary>
    public string OrderStatus { get; }

    /// <summary>
    /// 订单买家的姓名
    /// </summary>
    public string BuyerName { get; }

    /// <summary>
    /// 创建一个新的订单已发货状态变更事件
    /// </summary>
    /// <param name="orderId">订单ID</param>
    /// <param name="orderStatus">订单状态</param>
    /// <param name="buyerName">买家姓名</param>
    public OrderStatusChangedToShippedIntegrationEvent(int orderId, string orderStatus, string buyerName)
    {
        OrderId = orderId;
        OrderStatus = orderStatus;
        BuyerName = buyerName;
    }
}

