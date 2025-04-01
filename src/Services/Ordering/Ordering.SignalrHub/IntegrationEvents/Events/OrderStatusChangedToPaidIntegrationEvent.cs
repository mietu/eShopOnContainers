namespace Microsoft.eShopOnContainers.Services.Ordering.SignalrHub.IntegrationEvents.Events;

/// <summary>
/// 表示订单状态变更为已支付的集成事件
/// 当订单支付完成时，此事件被发布，用于通知相关服务处理后续逻辑
/// 继承自 IntegrationEvent 基类，包含事件标识和创建时间信息
/// </summary>
public record OrderStatusChangedToPaidIntegrationEvent : IntegrationEvent
{
    /// <summary>
    /// 获取订单ID
    /// </summary>
    public int OrderId { get; }

    /// <summary>
    /// 获取订单当前状态的描述
    /// </summary>
    public string OrderStatus { get; }

    /// <summary>
    /// 获取购买者姓名
    /// </summary>
    public string BuyerName { get; }

    /// <summary>
    /// 创建订单状态变更为已支付的集成事件实例
    /// </summary>
    /// <param name="orderId">订单ID</param>
    /// <param name="orderStatus">订单状态</param>
    /// <param name="buyerName">购买者姓名</param>
    public OrderStatusChangedToPaidIntegrationEvent(int orderId,
        string orderStatus, string buyerName)
    {
        OrderId = orderId;
        OrderStatus = orderStatus;
        BuyerName = buyerName;
    }
}
