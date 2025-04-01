namespace Microsoft.eShopOnContainers.Services.Ordering.SignalrHub.IntegrationEvents.EventHandling;

/// <summary>
/// 订单状态变更为"已支付"集成事件的处理器
/// 该处理器通过SignalR向客户端发送通知，使客户端实时更新订单状态
/// </summary>
public class OrderStatusChangedToPaidIntegrationEventHandler : IIntegrationEventHandler<OrderStatusChangedToPaidIntegrationEvent>
{
    private readonly IHubContext<NotificationsHub> _hubContext; // SignalR Hub上下文，用于向客户端发送通知
    private readonly ILogger<OrderStatusChangedToPaidIntegrationEventHandler> _logger; // 日志记录器

    /// <summary>
    /// 构造函数，通过依赖注入初始化所需服务
    /// </summary>
    /// <param name="hubContext">SignalR Hub上下文，用于向客户端推送消息</param>
    /// <param name="logger">日志记录器，用于记录处理事件的相关信息</param>
    /// <exception cref="ArgumentNullException">当任一依赖项为null时抛出</exception>
    public OrderStatusChangedToPaidIntegrationEventHandler(
        IHubContext<NotificationsHub> hubContext,
        ILogger<OrderStatusChangedToPaidIntegrationEventHandler> logger)
    {
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 处理订单状态变更为"已支付"的事件
    /// </summary>
    /// <param name="event">包含订单状态变更信息的事件对象</param>
    /// <returns>表示异步操作的任务</returns>
    public async Task Handle(OrderStatusChangedToPaidIntegrationEvent @event)
    {
        using (LogContext.PushProperty("IntegrationEventContext", $"{@event.Id}-{Program.AppName}"))
        {
            _logger.LogInformation("----- Handling integration event: {IntegrationEventId} at {AppName} - ({@IntegrationEvent})", @event.Id, Program.AppName, @event);

            // 向特定买家的SignalR组发送订单状态更新通知
            await _hubContext.Clients
                .Group(@event.BuyerName) // 使用买家名称作为组名，确保消息只发送给相关用户
                .SendAsync("UpdatedOrderState", new { OrderId = @event.OrderId, Status = @event.OrderStatus });
        }
    }
}
