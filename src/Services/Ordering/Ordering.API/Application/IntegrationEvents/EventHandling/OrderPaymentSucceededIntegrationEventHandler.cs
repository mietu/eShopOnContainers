namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.IntegrationEvents.EventHandling;

/// <summary>
/// 订单支付成功的集成事件处理程序，用于响应支付成功事件并更新订单状态。
/// </summary>
public class OrderPaymentSucceededIntegrationEventHandler :
    IIntegrationEventHandler<OrderPaymentSucceededIntegrationEvent>
{
    // Mediator 对象，用于发送命令
    private readonly IMediator _mediator;
    // Logger 对象，用于记录日志
    private readonly ILogger<OrderPaymentSucceededIntegrationEventHandler> _logger;

    /// <summary>
    /// 构造函数，初始化 OrderPaymentSucceededIntegrationEventHandler 实例。
    /// </summary>
    /// <param name="mediator">用于发送命令的 mediator 实例</param>
    /// <param name="logger">用于记录日志的 logger 实例</param>
    public OrderPaymentSucceededIntegrationEventHandler(
        IMediator mediator,
        ILogger<OrderPaymentSucceededIntegrationEventHandler> logger)
    {
        // 若 mediator 为 null，则抛出异常
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        // 若 logger 为 null，则抛出异常
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 处理订单支付成功的集成事件。
    /// </summary>
    /// <param name="event">订单支付成功事件，包含订单编号等信息</param>
    public async Task Handle(OrderPaymentSucceededIntegrationEvent @event)
    {
        // 在日志上下文中添加 IntegrationEventContext 属性，
        // 用于跟踪当前处理的集成事件的 ID 和应用程序名称
        using (LogContext.PushProperty("IntegrationEventContext", $"{@event.Id}-{Program.AppName}"))
        {
            // 记录处理事件的开始日志信息
            _logger.LogInformation("----- Handling integration event: {IntegrationEventId} at {AppName} - ({@IntegrationEvent})",
                @event.Id, Program.AppName, @event);

            // 构造命令，设置订单状态为已支付
            var command = new SetPaidOrderStatusCommand(@event.OrderId);

            // 记录发送命令的日志信息，包括命令名称和订单编号
            _logger.LogInformation(
                "----- Sending command: {CommandName} - {IdProperty}: {CommandId} ({@Command})",
                command.GetGenericTypeName(),
                nameof(command.OrderNumber),
                command.OrderNumber,
                command);

            // 通过 Mediator 发送命令，并等待命令处理完成
            await _mediator.Send(command);
        }
    }
}
