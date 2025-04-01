namespace Microsoft.eShopOnContainers.Services.Ordering.SignalrHub.IntegrationEvents.EventHandling;

/// <summary>
/// 处理订单状态变为已取消的集成事件的处理器
/// 实现了IIntegrationEventHandler接口，专门处理OrderStatusChangedToCancelledIntegrationEvent类型的事件
/// </summary>
public class OrderStatusChangedToCancelledIntegrationEventHandler : IIntegrationEventHandler<OrderStatusChangedToCancelledIntegrationEvent>
{
    /// <summary>
    /// SignalR Hub上下文，用于向连接的客户端发送实时通知
    /// </summary>
    private readonly IHubContext<NotificationsHub> _hubContext;

    /// <summary>
    /// 日志记录器，用于记录事件处理过程
    /// </summary>
    private readonly ILogger<OrderStatusChangedToCancelledIntegrationEventHandler> _logger;

    /// <summary>
    /// 构造函数，通过依赖注入获取所需服务
    /// </summary>
    /// <param name="hubContext">SignalR Hub上下文</param>
    /// <param name="logger">日志记录器</param>
    /// <exception cref="ArgumentNullException">当传入的参数为null时抛出</exception>
    public OrderStatusChangedToCancelledIntegrationEventHandler(
        IHubContext<NotificationsHub> hubContext,
        ILogger<OrderStatusChangedToCancelledIntegrationEventHandler> logger)
    {
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 处理订单状态变为已取消的集成事件
    /// </summary>
    /// <param name="event">包含订单取消相关信息的集成事件</param>
    /// <returns>表示异步操作的任务</returns>
    public async Task Handle(OrderStatusChangedToCancelledIntegrationEvent @event)
    {
        // 使用LogContext.PushProperty添加结构化日志上下文，便于跟踪特定事件的处理过程
        using (LogContext.PushProperty("IntegrationEventContext", $"{@event.Id}-{Program.AppName}"))
        {
            // 记录正在处理的集成事件信息
            _logger.LogInformation("----- Handling integration event: {IntegrationEventId} at {AppName} - ({@IntegrationEvent})",
                                  @event.Id, Program.AppName, @event);

            // 通过SignalR向特定买家组发送订单状态更新通知
            // Group方法选择接收通知的客户端组（基于买家名称）
            // SendAsync方法向客户端发送带有订单ID和状态的实时通知
            await _hubContext.Clients
                .Group(@event.BuyerName)
                .SendAsync("UpdatedOrderState", new { OrderId = @event.OrderId, Status = @event.OrderStatus });
        }
    }
}
