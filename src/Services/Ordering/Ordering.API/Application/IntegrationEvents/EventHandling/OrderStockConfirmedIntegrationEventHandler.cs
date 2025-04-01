namespace Ordering.API.Application.IntegrationEvents.EventHandling;

// OrderStockConfirmedIntegrationEventHandler 用于处理 OrderStockConfirmedIntegrationEvent 事件
public class OrderStockConfirmedIntegrationEventHandler :
    IIntegrationEventHandler<OrderStockConfirmedIntegrationEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrderStockConfirmedIntegrationEventHandler> _logger;

    // 构造函数：注入 IMediator 和 ILogger 服务
    public OrderStockConfirmedIntegrationEventHandler(
        IMediator mediator,
        ILogger<OrderStockConfirmedIntegrationEventHandler> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Handle 方法：处理接收到的 OrderStockConfirmedIntegrationEvent 事件
    public async Task Handle(OrderStockConfirmedIntegrationEvent @event)
    {
        // 使用 LogContext.PushProperty 为日志添加额外的上下文信息，该信息包含事件的唯一 Id 与应用名称
        using (LogContext.PushProperty("IntegrationEventContext", $"{@event.Id}-{Program.AppName}"))
        {
            // 日志记录：指出正在处理集成事件
            _logger.LogInformation("----- Handling integration event: {IntegrationEventId} at {AppName} - ({@IntegrationEvent})",
                @event.Id, Program.AppName, @event);

            // 创建命令对象，用于设置订单状态为 StockConfirmed
            var command = new SetStockConfirmedOrderStatusCommand(@event.OrderId);

            // 日志记录：记录发送命令的信息
            _logger.LogInformation(
                "----- Sending command: {CommandName} - {IdProperty}: {CommandId} ({@Command})",
                command.GetGenericTypeName(),
                nameof(command.OrderNumber),
                command.OrderNumber,
                command);

            // 通过 Mediator 发送命令，执行业务逻辑
            await _mediator.Send(command);
        }
    }
}
