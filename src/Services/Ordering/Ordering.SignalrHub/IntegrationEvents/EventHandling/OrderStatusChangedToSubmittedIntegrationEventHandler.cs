namespace Microsoft.eShopOnContainers.Services.Ordering.SignalrHub.IntegrationEvents.EventHandling;

/// <summary>
/// 处理订单状态变更为"已提交"的集成事件处理程序
/// 当订单状态变成已提交时，此处理程序通过SignalR向客户端推送通知
/// </summary>
public class OrderStatusChangedToSubmittedIntegrationEventHandler :
    IIntegrationEventHandler<OrderStatusChangedToSubmittedIntegrationEvent>
{
    private readonly IHubContext<NotificationsHub> _hubContext; // SignalR Hub上下文，用于向客户端发送消息
    private readonly ILogger<OrderStatusChangedToSubmittedIntegrationEventHandler> _logger; // 日志记录器

    /// <summary>
    /// 构造函数，通过依赖注入接收所需依赖
    /// </summary>
    /// <param name="hubContext">SignalR Hub上下文，用于向客户端推送消息</param>
    /// <param name="logger">日志记录器，用于记录处理事件的日志</param>
    /// <exception cref="ArgumentNullException">当任何参数为null时抛出</exception>
    public OrderStatusChangedToSubmittedIntegrationEventHandler(
        IHubContext<NotificationsHub> hubContext,
        ILogger<OrderStatusChangedToSubmittedIntegrationEventHandler> logger)
    {
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 处理订单状态变更为已提交的集成事件
    /// </summary>
    /// <param name="event">包含订单状态变更信息的集成事件</param>
    /// <returns>异步任务</returns>
    public async Task Handle(OrderStatusChangedToSubmittedIntegrationEvent @event)
    {
        // 使用LogContext添加集成事件上下文信息到日志中
        using (LogContext.PushProperty("IntegrationEventContext", $"{@event.Id}-{Program.AppName}"))
        {
            // 记录处理集成事件的日志
            _logger.LogInformation("----- 处理集成事件: {IntegrationEventId} 在 {AppName} - ({@IntegrationEvent})",
                @event.Id, Program.AppName, @event);

            // 通过SignalR向特定用户组发送订单状态更新通知
            // 组名为买家姓名，确保只有相关用户收到通知
            await _hubContext.Clients
                .Group(@event.BuyerName)
                .SendAsync("UpdatedOrderState", new { OrderId = @event.OrderId, Status = @event.OrderStatus });
        }
    }
}
