namespace Microsoft.eShopOnContainers.Services.Catalog.API.IntegrationEvents.Events;

/// <summary>
/// 表示订单库存确认的集成事件。
/// 继承自 IntegrationEvent 基类，集成事件基类包含事件的唯一标识和创建时间等公共属性。
/// </summary>
public record OrderStockConfirmedIntegrationEvent : IntegrationEvent
{
    /// <summary>
    /// 获取订单的唯一标识。订单处理过程中，该 Id 用于标识具体的订单。
    /// </summary>
    public int OrderId { get; }

    /// <summary>
    /// 构造函数，初始化 OrderStockConfirmedIntegrationEvent 实例并设置订单编号。
    /// </summary>
    /// <param name="orderId">订单的唯一编号</param>
    public OrderStockConfirmedIntegrationEvent(int orderId) => OrderId = orderId;
}
