namespace Microsoft.eShopOnContainers.Services.Ordering.SignalrHub.IntegrationEvents;

/// <summary>
/// 订单状态变更为"等待验证"的集成事件
/// 当订单状态变更为等待验证时触发，用于通知相关服务
/// </summary>
public record OrderStatusChangedToAwaitingValidationIntegrationEvent : IntegrationEvent
{
    /// <summary>
    /// 订单ID
    /// </summary>
    public int OrderId { get; }

    /// <summary>
    /// 订单状态
    /// </summary>
    public string OrderStatus { get; }

    /// <summary>
    /// 买家姓名
    /// </summary>
    public string BuyerName { get; }

    /// <summary>
    /// 创建一个新的订单状态变更为"等待验证"的集成事件
    /// </summary>
    /// <param name="orderId">订单ID</param>
    /// <param name="orderStatus">订单状态</param>
    /// <param name="buyerName">买家姓名</param>
    public OrderStatusChangedToAwaitingValidationIntegrationEvent(int orderId, string orderStatus, string buyerName)
    {
        OrderId = orderId;
        OrderStatus = orderStatus;
        BuyerName = buyerName;
    }
}

