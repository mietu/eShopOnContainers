namespace Microsoft.eShopOnContainers.Payment.API.IntegrationEvents.Events;

/// <summary>
/// 订单状态变更为库存已确认的集成事件
/// 此事件在订单服务中的库存确认后发布，用于通知支付服务可以开始处理支付
/// </summary>
public record OrderStatusChangedToStockConfirmedIntegrationEvent : IntegrationEvent
{
    /// <summary>
    /// 需要处理支付的订单ID
    /// </summary>
    public int OrderId { get; }

    /// <summary>
    /// 创建一个新的订单状态变更为库存已确认的集成事件实例
    /// </summary>
    /// <param name="orderId">需要处理支付的订单ID</param>
    public OrderStatusChangedToStockConfirmedIntegrationEvent(int orderId)
        => OrderId = orderId;
}
