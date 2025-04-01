namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.IntegrationEvents.Events;

/// <summary>
/// 此记录类型表示确认宽限期结束的集成事件。
/// 继承自 IntegrationEvent 基类，可以包含全局唯一的标识符和创建时间属性。
/// </summary>
public record GracePeriodConfirmedIntegrationEvent : IntegrationEvent
{
    /// <summary>
    /// 订单ID，用于标识与此事件相关联的订单。
    /// </summary>
    public int OrderId { get; }

    /// <summary>
    /// 构造函数，初始化订单ID。
    /// </summary>
    /// <param name="orderId">订单ID</param>
    public GracePeriodConfirmedIntegrationEvent(int orderId) =>
        OrderId = orderId;
}

