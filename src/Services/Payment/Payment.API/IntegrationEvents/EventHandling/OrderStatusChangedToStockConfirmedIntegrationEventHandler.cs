namespace Microsoft.eShopOnContainers.Payment.API.IntegrationEvents.EventHandling;

/// <summary>
/// 用于处理订单状态变更为"库存已确认"的集成事件处理器
/// 此处理器负责在库存确认后模拟支付处理流程，并发布相应的支付结果事件
/// </summary>
public class OrderStatusChangedToStockConfirmedIntegrationEventHandler :
    IIntegrationEventHandler<OrderStatusChangedToStockConfirmedIntegrationEvent>
{
    private readonly IEventBus _eventBus;                // 集成事件总线，用于发布支付结果事件
    private readonly PaymentSettings _settings;          // 支付设置，包含支付是否成功的配置
    private readonly ILogger<OrderStatusChangedToStockConfirmedIntegrationEventHandler> _logger;  // 日志记录器

    /// <summary>
    /// 构造函数，通过依赖注入接收所需的服务
    /// </summary>
    /// <param name="eventBus">事件总线，用于发布支付结果事件</param>
    /// <param name="settings">支付设置，包含支付是否成功的配置</param>
    /// <param name="logger">日志记录器，用于记录处理过程</param>
    /// <exception cref="System.ArgumentNullException">当logger参数为null时抛出</exception>
    public OrderStatusChangedToStockConfirmedIntegrationEventHandler(
        IEventBus eventBus,
        IOptionsSnapshot<PaymentSettings> settings,
        ILogger<OrderStatusChangedToStockConfirmedIntegrationEventHandler> logger)
    {
        _eventBus = eventBus;
        _settings = settings.Value;
        _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));

        _logger.LogTrace("PaymentSettings: {@PaymentSettings}", _settings);
    }

    /// <summary>
    /// 处理订单状态变更为"库存已确认"的集成事件
    /// 根据配置模拟支付处理，并发布相应的支付结果事件
    /// </summary>
    /// <param name="event">包含订单信息的集成事件</param>
    /// <returns>表示异步操作的任务</returns>
    public async Task Handle(OrderStatusChangedToStockConfirmedIntegrationEvent @event)
    {
        using (LogContext.PushProperty("IntegrationEventContext", $"{@event.Id}-{Program.AppName}"))
        {
            _logger.LogInformation("----- 处理集成事件: {IntegrationEventId} 在 {AppName} - ({@IntegrationEvent})", @event.Id, Program.AppName, @event);

            IntegrationEvent orderPaymentIntegrationEvent;

            // 业务功能说明:
            // 当处理 OrderStatusChangedToStockConfirmed 集成事件时
            // 这里我们模拟对接支付网关进行支付处理
            // 实际上我们没有进行真实支付，而是通过环境变量配置来模拟支付结果
            // 支付可能成功也可能失败

            if (_settings.PaymentSucceeded)
            {
                // 模拟支付成功，创建支付成功事件
                orderPaymentIntegrationEvent = new OrderPaymentSucceededIntegrationEvent(@event.OrderId);
            }
            else
            {
                // 模拟支付失败，创建支付失败事件
                orderPaymentIntegrationEvent = new OrderPaymentFailedIntegrationEvent(@event.OrderId);
            }

            _logger.LogInformation("----- 发布集成事件: {IntegrationEventId} 从 {AppName} - ({@IntegrationEvent})",
                orderPaymentIntegrationEvent.Id, Program.AppName, orderPaymentIntegrationEvent);

            // 通过事件总线发布支付结果事件
            _eventBus.Publish(orderPaymentIntegrationEvent);

            await Task.CompletedTask;
        }
    }
}
