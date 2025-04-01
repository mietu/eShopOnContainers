namespace Ordering.BackgroundTasks.Events
{
    using Microsoft.eShopOnContainers.BuildingBlocks.EventBus.Events;

    /// <summary>
    /// 订单宽限期确认集成事件
    /// 用于在系统各服务间传递订单已确认宽限期结束的事件信息
    /// 此事件继承自 IntegrationEvent 基类，包含了事件基本属性，如唯一标识和创建时间
    /// </summary>
    public record GracePeriodConfirmedIntegrationEvent : IntegrationEvent
    {
        /// <summary>
        /// 订单标识，用于确认具体哪个订单触发了此事件
        /// </summary>
        public int OrderId { get; }

        /// <summary>
        /// 构造函数，初始化订单宽限期确认事件实例并设置订单标识
        /// </summary>
        /// <param name="orderId">订单的唯一标识</param>
        public GracePeriodConfirmedIntegrationEvent(int orderId) =>
            OrderId = orderId;
    }
}
