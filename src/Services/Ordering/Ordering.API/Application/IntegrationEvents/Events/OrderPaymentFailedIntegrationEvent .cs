namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.IntegrationEvents.Events
{
    /// <summary>
    /// 表示订单支付失败的集成事件。
    /// 当订单支付失败时，将触发该事件用于系统间通信。
    /// 继承自 IntegrationEvent，包含基础的事件标识和创建时间信息。
    /// </summary>
    public record OrderPaymentFailedIntegrationEvent : IntegrationEvent
    {
        /// <summary>
        /// 获取订单的唯一标识符。
        /// </summary>
        public int OrderId { get; }

        /// <summary>
        /// 使用订单ID初始化 OrderPaymentFailedIntegrationEvent 实例。
        /// </summary>
        /// <param name="orderId">订单的唯一标识符</param>
        public OrderPaymentFailedIntegrationEvent(int orderId) => OrderId = orderId;
    }
}
