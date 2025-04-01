namespace Microsoft.eShopOnContainers.Services.Ordering.SignalrHub.IntegrationEvents.Events;

/// <summary>
/// 表示订单状态变更为"已提交"的集成事件
/// 当订单成功提交时触发此事件，用于通知其他服务和客户端
/// </summary>
public record OrderStatusChangedToSubmittedIntegrationEvent : IntegrationEvent
{
    /// <summary>
    /// 获取订单ID
    /// </summary>
    public int OrderId { get; }

    /// <summary>
    /// 获取订单状态
    /// </summary>
    public string OrderStatus { get; }

    /// <summary>
    /// 获取买家姓名
    /// </summary>
    public string BuyerName { get; }

    /// <summary>
    /// 初始化 <see cref="OrderStatusChangedToSubmittedIntegrationEvent"/> 类的新实例
    /// </summary>
    /// <param name="orderId">订单ID</param>
    /// <param name="orderStatus">订单状态</param>
    /// <param name="buyerName">买家姓名</param>
    public OrderStatusChangedToSubmittedIntegrationEvent(int orderId, string orderStatus, string buyerName)
    {
        OrderId = orderId;
        OrderStatus = orderStatus;
        BuyerName = buyerName;
    }
}
