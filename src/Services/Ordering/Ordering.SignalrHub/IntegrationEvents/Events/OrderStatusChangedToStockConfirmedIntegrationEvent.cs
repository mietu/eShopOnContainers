namespace Microsoft.eShopOnContainers.Services.Ordering.SignalrHub.IntegrationEvents.Events;

/// <summary>
/// 库存确认后订单状态变更的集成事件
/// 当订单状态变更为库存已确认时，此事件将被发布
/// </summary>
public record OrderStatusChangedToStockConfirmedIntegrationEvent : IntegrationEvent
{
    /// <summary>
    /// 订单ID
    /// </summary>
    public int OrderId { get; }

    /// <summary>
    /// 订单状态
    /// </summary>
    public string OrderStatus { get; }

    /// <summary>
    /// 买家姓名
    /// </summary>
    public string BuyerName { get; }

    /// <summary>
    /// 创建一个新的库存确认状态变更事件实例
    /// </summary>
    /// <param name="orderId">订单ID</param>
    /// <param name="orderStatus">订单状态</param>
    /// <param name="buyerName">买家姓名</param>
    public OrderStatusChangedToStockConfirmedIntegrationEvent(int orderId, string orderStatus, string buyerName)
    {
        OrderId = orderId;
        OrderStatus = orderStatus;
        BuyerName = buyerName;
    }
}
