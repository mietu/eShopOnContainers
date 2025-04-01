namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.IntegrationEvents.EventHandling;

// 本类实现了 IIntegrationEventHandler<UserCheckoutAcceptedIntegrationEvent> 接口，
// 用于处理用户结账成功后的集成事件，从而启动创建订单流程
public class UserCheckoutAcceptedIntegrationEventHandler : IIntegrationEventHandler<UserCheckoutAcceptedIntegrationEvent>
{
    // Mediator 用于发送命令，协调不同模块的交互
    private readonly IMediator _mediator;
    // Logger 用于记录日志，方便追踪事件处理过程
    private readonly ILogger<UserCheckoutAcceptedIntegrationEventHandler> _logger;

    // 构造函数进行依赖注入，确保 mediator 和 logger 均不为空
    public UserCheckoutAcceptedIntegrationEventHandler(
        IMediator mediator,
        ILogger<UserCheckoutAcceptedIntegrationEventHandler> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 当接收到 UserCheckoutAcceptedIntegrationEvent 时，启动订单创建过程
    /// 此方法通过 Mediator 发送 CreateOrderCommand 命令
    /// </summary>
    /// <param name="@event">
    /// 集成事件对象，由 basket.api 成功处理订单商品后发送，该对象封装了结账所需的所有信息
    /// </param>
    /// <returns></returns>
    public async Task Handle(UserCheckoutAcceptedIntegrationEvent @event)
    {
        // 使用 LogContext.PushProperty 为当前日志上下文添加集成事件标识属性
        using (LogContext.PushProperty("IntegrationEventContext", $"{@event.Id}-{Program.AppName}"))
        {
            // 记录开始处理集成事件的日志，包含事件 ID、应用名和事件详情
            _logger.LogInformation("----- Handling integration event: {IntegrationEventId} at {AppName} - ({@IntegrationEvent})",
                @event.Id, Program.AppName, @event);

            // 初始化订单创建结果标识
            var result = false;

            // 检查 RequestId 是否有效（非 Guid.Empty 表示有效）
            if (@event.RequestId != Guid.Empty)
            {
                // 为请求命令建立一个日志上下文属性，用于补充唯一标识
                using (LogContext.PushProperty("IdentifiedCommandId", @event.RequestId))
                {
                    // 根据集成事件中的信息构造创建订单的命令对象
                    var createOrderCommand = new CreateOrderCommand(
                        @event.Basket.Items,
                        @event.UserId,
                        @event.UserName,
                        @event.City,
                        @event.Street,
                        @event.State,
                        @event.Country,
                        @event.ZipCode,
                        @event.CardNumber,
                        @event.CardHolderName,
                        @event.CardExpiration,
                        @event.CardSecurityNumber,
                        @event.CardTypeId);

                    // 将命令和事件中的 RequestId 封装到 IdentifiedCommand 对象中
                    var requestCreateOrder = new IdentifiedCommand<CreateOrderCommand, bool>(createOrderCommand, @event.RequestId);

                    // 记录发送命令的日志，包括命令名称、标识属性以及命令详情
                    _logger.LogInformation(
                        "----- Sending command: {CommandName} - {IdProperty}: {CommandId} ({@Command})",
                        requestCreateOrder.GetGenericTypeName(),
                        nameof(requestCreateOrder.Id),
                        requestCreateOrder.Id,
                        requestCreateOrder);

                    // 通过 Mediator 发送命令，并等待处理结果
                    result = await _mediator.Send(requestCreateOrder);

                    // 根据返回结果记录处理成功或失败的日志信息
                    if (result)
                    {
                        _logger.LogInformation("----- CreateOrderCommand suceeded - RequestId: {RequestId}", @event.RequestId);
                    }
                    else
                    {
                        _logger.LogWarning("CreateOrderCommand failed - RequestId: {RequestId}", @event.RequestId);
                    }
                }
            }
            else
            {
                // 如果 RequestId 无效，则记录警告日志
                _logger.LogWarning("Invalid IntegrationEvent - RequestId is missing - {@IntegrationEvent}", @event);
            }
        }
    }
}
