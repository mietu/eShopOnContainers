namespace Basket.API.IntegrationEvents.EventHandling;

/// <summary>
/// 处理 OrderStartedIntegrationEvent 集成事件的处理器
/// 当订单启动时，删除与该订单关联的用户购物篮
/// </summary>
public class OrderStartedIntegrationEventHandler : IIntegrationEventHandler<OrderStartedIntegrationEvent>
{
    private readonly IBasketRepository _repository;
    private readonly ILogger<OrderStartedIntegrationEventHandler> _logger;

    /// <summary>
    /// 构造函数，初始化 OrderStartedIntegrationEventHandler 实例
    /// </summary>
    /// <param name="repository">购物篮仓储，用于数据操作</param>
    /// <param name="logger">日志记录器，用于记录事件处理日志</param>
    public OrderStartedIntegrationEventHandler(
        IBasketRepository repository,
        ILogger<OrderStartedIntegrationEventHandler> logger)
    {
        // 确保仓储对象不为空，否则抛出异常
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        // 确保日志记录器不为空，否则抛出异常
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 处理 OrderStartedIntegrationEvent 事件
    /// </summary>
    /// <param name="event">触发的订单启动事件</param>
    public async Task Handle(OrderStartedIntegrationEvent @event)
    {
        // 使用 LogContext 添加集成事件上下文信息，便于追踪日志
        using (LogContext.PushProperty("IntegrationEventContext", $"{@event.Id}-{Program.AppName}"))
        {
            // 记录接收并处理集成事件的信息
            _logger.LogInformation("----- Handling integration event: {IntegrationEventId} at {AppName} - ({@IntegrationEvent})", @event.Id, Program.AppName, @event);

            // 调用仓储方法，删除与该事件关联的用户购物篮
            await _repository.DeleteBasketAsync(@event.UserId.ToString());
        }
    }
}




