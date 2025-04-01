namespace Microsoft.eShopOnContainers.Services.Ordering.SignalrHub.IntegrationEvents;

/// <summary>
/// 订单状态变更为"等待验证"事件的处理器
/// 负责将订单状态变更信息通过SignalR实时推送给客户端
/// </summary>
public class OrderStatusChangedToAwaitingValidationIntegrationEventHandler : IIntegrationEventHandler<OrderStatusChangedToAwaitingValidationIntegrationEvent>
{
    private readonly IHubContext<NotificationsHub> _hubContext; // SignalR集线器上下文，用于向客户端发送消息
    private readonly ILogger<OrderStatusChangedToAwaitingValidationIntegrationEventHandler> _logger; // 日志记录器

    /// <summary>
    /// 构造函数，通过依赖注入获取所需服务
    /// </summary>
    /// <param name="hubContext">SignalR集线器上下文，用于向客户端推送消息</param>
    /// <param name="logger">日志记录器，用于记录事件处理过程</param>
    /// <exception cref="ArgumentNullException">当任何参数为null时抛出</exception>
    public OrderStatusChangedToAwaitingValidationIntegrationEventHandler(
        IHubContext<NotificationsHub> hubContext,
        ILogger<OrderStatusChangedToAwaitingValidationIntegrationEventHandler> logger)
    {
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 处理订单状态变更为"等待验证"的集成事件
    /// </summary>
    /// <param name="event">包含订单状态变更详情的集成事件</param>
    /// <returns>表示异步操作的任务</returns>
    public async Task Handle(OrderStatusChangedToAwaitingValidationIntegrationEvent @event)
    {
        using (LogContext.PushProperty("IntegrationEventContext", $"{@event.Id}-{Program.AppName}"))
        {
            _logger.LogInformation("----- 处理集成事件: {IntegrationEventId} 在 {AppName} - ({@IntegrationEvent})", @event.Id, Program.AppName, @event);

            // 向指定买家组发送订单状态更新通知
            await _hubContext.Clients
                .Group(@event.BuyerName)
                .SendAsync("UpdatedOrderState", new { OrderId = @event.OrderId, Status = @event.OrderStatus });
        }
    }
}
