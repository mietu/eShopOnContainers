namespace Microsoft.eShopOnContainers.Payment.API.IntegrationEvents.Events;

/// <summary>
/// 订单支付失败集成事件
/// 当支付处理失败时触发此事件，用于通知其他服务（如订单服务）支付失败的情况
/// </summary>
public record OrderPaymentFailedIntegrationEvent : IntegrationEvent
{
    /// <summary>
    /// 获取失败支付关联的订单ID
    /// </summary>
    public int OrderId { get; }

    /// <summary>
    /// 创建一个新的订单支付失败事件实例
    /// </summary>
    /// <param name="orderId">失败支付的订单ID</param>
    public OrderPaymentFailedIntegrationEvent(int orderId) => OrderId = orderId;
}
