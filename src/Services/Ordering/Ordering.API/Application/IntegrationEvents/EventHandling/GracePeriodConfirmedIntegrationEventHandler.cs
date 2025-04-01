namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.IntegrationEvents.EventHandling;

/// <summary>
/// 处理确认宽限期完成的集成事件的事件处理器
/// </summary>
public class GracePeriodConfirmedIntegrationEventHandler : IIntegrationEventHandler<GracePeriodConfirmedIntegrationEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<GracePeriodConfirmedIntegrationEventHandler> _logger;

    /// <summary>
    /// 构造函数，注入 mediator 和 logger 实例
    /// </summary>
    /// <param name="mediator">用于发送命令的 mediator</param>
    /// <param name="logger">用于记录日志的 logger</param>
    public GracePeriodConfirmedIntegrationEventHandler(
        IMediator mediator,
        ILogger<GracePeriodConfirmedIntegrationEventHandler> logger)
    {
        _mediator = mediator;
        // logger 不能为空，否则抛出参数为空异常
        _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 处理集成事件，确认宽限期已结束，订单将继续进行验证处理
    /// </summary>
    /// <param name="event">待处理的集成事件对象</param>
    /// <returns>异步任务</returns>
    public async Task Handle(GracePeriodConfirmedIntegrationEvent @event)
    {
        // 使用 LogContext 添加上下文属性以便跟踪日志
        using (LogContext.PushProperty("IntegrationEventContext", $"{@event.Id}-{Program.AppName}"))
        {
            // 记录处理事件的日志信息，包括事件ID、应用名称和事件对象详情
            _logger.LogInformation("----- Handling integration event: {IntegrationEventId} at {AppName} - ({@IntegrationEvent})", @event.Id, Program.AppName, @event);

            // 创建设置订单状态为待验证的命令，传入订单ID
            var command = new SetAwaitingValidationOrderStatusCommand(@event.OrderId);

            // 记录发送命令的日志信息，包含命令名称、订单编号等信息
            _logger.LogInformation(
                "----- Sending command: {CommandName} - {IdProperty}: {CommandId} ({@Command})",
                command.GetGenericTypeName(),
                nameof(command.OrderNumber),
                command.OrderNumber,
                command);

            // 使用 mediator 发送命令，继续订单流程的处理
            await _mediator.Send(command);
        }
    }
}
