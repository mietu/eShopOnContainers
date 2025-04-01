namespace Microsoft.eShopOnContainers.Services.Ordering.SignalrHub.IntegrationEvents.EventHandling;

/// <summary>
/// 处理订单状态变更为"库存已确认"的集成事件处理器
/// 实现了IIntegrationEventHandler接口，用于接收和处理特定类型的集成事件
/// </summary>
public class OrderStatusChangedToStockConfirmedIntegrationEventHandler :
    IIntegrationEventHandler<OrderStatusChangedToStockConfirmedIntegrationEvent>
{
    private readonly IHubContext<NotificationsHub> _hubContext; // SignalR Hub上下文，用于向客户端推送消息
    private readonly ILogger<OrderStatusChangedToStockConfirmedIntegrationEventHandler> _logger; // 日志记录器

    /// <summary>
    /// 构造函数，通过依赖注入接收所需的服务
    /// </summary>
    /// <param name="hubContext">SignalR Hub上下文，用于实时通知客户端</param>
    /// <param name="logger">日志记录器，用于记录处理事件的信息</param>
    /// <exception cref="ArgumentNullException">当任何参数为null时抛出</exception>
    public OrderStatusChangedToStockConfirmedIntegrationEventHandler(
        IHubContext<NotificationsHub> hubContext,
        ILogger<OrderStatusChangedToStockConfirmedIntegrationEventHandler> logger)
    {
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 处理订单状态变更为"库存已确认"的集成事件
    /// </summary>
    /// <param name="event">包含订单信息和状态的集成事件</param>
    /// <returns>一个表示异步操作的任务</returns>
    public async Task Handle(OrderStatusChangedToStockConfirmedIntegrationEvent @event)
    {
        using (LogContext.PushProperty("IntegrationEventContext", $"{@event.Id}-{Program.AppName}"))
        {
            // 记录正在处理的集成事件信息
            _logger.LogInformation("----- Handling integration event: {IntegrationEventId} at {AppName} - ({@IntegrationEvent})",
                @event.Id, Program.AppName, @event);

            // 通过SignalR向特定买家组发送订单状态更新通知
            // Group方法指定了接收消息的用户组（按买家名称分组）
            // SendAsync方法向客户端发送名为"UpdatedOrderState"的消息，包含订单ID和状态信息
            await _hubContext.Clients
                .Group(@event.BuyerName)
                .SendAsync("UpdatedOrderState", new { OrderId = @event.OrderId, Status = @event.OrderStatus });
        }
    }
}
