namespace Microsoft.eShopOnContainers.Services.Ordering.SignalrHub.IntegrationEvents.EventHandling;

/// <summary>
/// 处理订单状态变更为"已发货"的集成事件处理器
/// 当订单状态变为已发货时，通过SignalR向客户端推送通知
/// </summary>
public class OrderStatusChangedToShippedIntegrationEventHandler : IIntegrationEventHandler<OrderStatusChangedToShippedIntegrationEvent>
{
    private readonly IHubContext<NotificationsHub> _hubContext; // SignalR Hub上下文，用于向客户端发送实时通知
    private readonly ILogger<OrderStatusChangedToShippedIntegrationEventHandler> _logger; // 日志记录器

    /// <summary>
    /// 构造函数，通过依赖注入获取SignalR Hub上下文和日志记录器
    /// </summary>
    /// <param name="hubContext">SignalR Hub上下文，用于发送实时通知</param>
    /// <param name="logger">日志记录器</param>
    /// <exception cref="ArgumentNullException">当hubContext或logger为null时抛出</exception>
    public OrderStatusChangedToShippedIntegrationEventHandler(
        IHubContext<NotificationsHub> hubContext,
        ILogger<OrderStatusChangedToShippedIntegrationEventHandler> logger)
    {
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 处理订单状态变更为已发货的集成事件
    /// </summary>
    /// <param name="event">包含订单状态变更信息的集成事件</param>
    /// <returns>表示异步操作的任务</returns>
    public async Task Handle(OrderStatusChangedToShippedIntegrationEvent @event)
    {
        // 使用Serilog的LogContext将事件ID和应用名称添加到日志上下文中，方便跟踪
        using (LogContext.PushProperty("IntegrationEventContext", $"{@event.Id}-{Program.AppName}"))
        {
            // 记录处理集成事件的信息
            _logger.LogInformation("----- Handling integration event: {IntegrationEventId} at {AppName} - ({@IntegrationEvent})",
                @event.Id, Program.AppName, @event);

            // 通过SignalR向特定用户组(以买家名称标识)发送订单状态更新通知
            // 客户端需要监听"UpdatedOrderState"事件来接收此通知
            await _hubContext.Clients
                .Group(@event.BuyerName) // 按买家名称分组，确保通知只发送给相关用户
                .SendAsync("UpdatedOrderState", new { OrderId = @event.OrderId, Status = @event.OrderStatus });
        }
    }
}
