namespace Microsoft.eShopOnContainers.Payment.API.IntegrationEvents.Events;

/// <summary>
/// 订单支付成功集成事件
/// 当订单支付处理成功后，由支付服务发布此事件，通知其他相关服务（如订单服务）更新订单状态
/// </summary>
public record OrderPaymentSucceededIntegrationEvent : IntegrationEvent
{
    /// <summary>
    /// 获取已支付成功的订单ID
    /// </summary>
    public int OrderId { get; }

    /// <summary>
    /// 初始化订单支付成功集成事件的新实例
    /// </summary>
    /// <param name="orderId">已支付成功的订单ID</param>
    public OrderPaymentSucceededIntegrationEvent(int orderId) => OrderId = orderId;
}
