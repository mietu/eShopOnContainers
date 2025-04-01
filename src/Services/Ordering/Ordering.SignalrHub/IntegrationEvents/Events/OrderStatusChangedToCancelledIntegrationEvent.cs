namespace Microsoft.eShopOnContainers.Services.Ordering.SignalrHub.IntegrationEvents.Events;

/// <summary>
/// 订单状态变更为取消的集成事件
/// 当订单被取消时，通过此事件通知其他服务
/// </summary>
public record OrderStatusChangedToCancelledIntegrationEvent : IntegrationEvent
{
    /// <summary>
    /// 被取消的订单ID
    /// </summary>
    public int OrderId { get; }

    /// <summary>
    /// 订单当前状态描述
    /// </summary>
    public string OrderStatus { get; }

    /// <summary>
    /// 买家姓名，用于在通知中标识订单所属用户
    /// </summary>
    public string BuyerName { get; }

    /// <summary>
    /// 创建订单取消事件的构造函数
    /// </summary>
    /// <param name="orderId">被取消的订单ID</param>
    /// <param name="orderStatus">订单状态描述</param>
    /// <param name="buyerName">买家姓名</param>
    public OrderStatusChangedToCancelledIntegrationEvent(int orderId, string orderStatus, string buyerName)
    {
        OrderId = orderId;
        OrderStatus = orderStatus;
        BuyerName = buyerName;
    }
}

