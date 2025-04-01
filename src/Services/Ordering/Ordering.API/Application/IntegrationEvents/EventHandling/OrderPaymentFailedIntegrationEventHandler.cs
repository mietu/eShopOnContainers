namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.IntegrationEvents.EventHandling;

/// <summary>
/// 处理订单支付失败的集成事件的处理程序。
/// 当订单支付失败时，将触发此处理程序，进而发送取消订单的命令。
/// </summary>
public class OrderPaymentFailedIntegrationEventHandler : IIntegrationEventHandler<OrderPaymentFailedIntegrationEvent>
{
    // IMediator 用于发送命令
    private readonly IMediator _mediator;
    // ILogger 用于记录日志信息
    private readonly ILogger<OrderPaymentFailedIntegrationEventHandler> _logger;

    /// <summary>
    /// 构造函数，注入依赖的 IMediator 和 ILogger 实例。
    /// </summary>
    /// <param name="mediator">用于发送应用命令</param>
    /// <param name="logger">用于记录日志</param>
    public OrderPaymentFailedIntegrationEventHandler(
        IMediator mediator,
        ILogger<OrderPaymentFailedIntegrationEventHandler> logger)
    {
        // 检查 mediator 是否为 null
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        // 检查 logger 是否为 null
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 处理订单支付失败的集成事件。
    /// 记录事件信息后，创建并发送取消订单的命令。
    /// </summary>
    /// <param name="event">订单支付失败的集成事件</param>
    public async Task Handle(OrderPaymentFailedIntegrationEvent @event)
    {
        // 使用 LogContext 推送额外的上下文信息（集成事件ID与应用名称），便于日志跟踪
        using (LogContext.PushProperty("IntegrationEventContext", $"{@event.Id}-{Program.AppName}"))
        {
            // 记录处理集成事件的初始日志信息
            _logger.LogInformation(
                "----- Handling integration event: {IntegrationEventId} at {AppName} - ({@IntegrationEvent})",
                @event.Id,
                Program.AppName,
                @event);

            // 创建取消订单的命令，使用订单ID初始化
            var command = new CancelOrderCommand(@event.OrderId);

            // 记录发送命令前的日志信息
            _logger.LogInformation(
                "----- Sending command: {CommandName} - {IdProperty}: {CommandId} ({@Command})",
                command.GetGenericTypeName(),
                nameof(command.OrderNumber),  // 假定 OrderNumber 为命令中的标识属性
                command.OrderNumber,
                command);

            // 通过 IMediator 发送命令，触发相应的业务处理
            await _mediator.Send(command);
        }
    }
}
