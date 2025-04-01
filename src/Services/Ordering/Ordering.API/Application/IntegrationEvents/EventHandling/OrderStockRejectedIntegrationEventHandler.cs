namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.IntegrationEvents.EventHandling;
// 订单库存不足拒绝集成事件的处理程序，负责处理库存不足时的业务逻辑
public class OrderStockRejectedIntegrationEventHandler : IIntegrationEventHandler<OrderStockRejectedIntegrationEvent>
{
    // 调用中介者进行命令发送
    private readonly IMediator _mediator;
    // 日志记录器，用于日志输出和调试
    private readonly ILogger<OrderStockRejectedIntegrationEventHandler> _logger;

    // 构造函数注入依赖：中介者和日志记录器
    public OrderStockRejectedIntegrationEventHandler(
        IMediator mediator,
        ILogger<OrderStockRejectedIntegrationEventHandler> logger)
    {
        _mediator = mediator;
        // 检查 logger 是否为 null
        _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
    }

    // 处理库存不足拒绝的集成事件
    public async Task Handle(OrderStockRejectedIntegrationEvent @event)
    {
        // 使用LogContext为当前日志消息追加集成事件信息属性
        using (LogContext.PushProperty("IntegrationEventContext", $"{@event.Id}-{Program.AppName}"))
        {
            // 记录处理集成事件的日志信息
            _logger.LogInformation("----- Handling integration event: {IntegrationEventId} at {AppName} - ({@IntegrationEvent})",
                @event.Id, Program.AppName, @event);

            // 从事件中筛选出库存不足的订单项，提取对应产品ID
            var orderStockRejectedItems = @event.OrderStockItems
                .FindAll(c => !c.HasStock)
                .Select(c => c.ProductId)
                .ToList();

            // 创建命令对象，用于更新订单状态为库存不足拒绝状态
            var command = new SetStockRejectedOrderStatusCommand(@event.OrderId, orderStockRejectedItems);

            // 记录发送命令的日志信息，便于跟踪命令的流转与执行情况
            _logger.LogInformation(
                "----- Sending command: {CommandName} - {IdProperty}: {CommandId} ({@Command})",
                command.GetGenericTypeName(),
                nameof(command.OrderNumber),
                command.OrderNumber,
                command);

            // 使用中介者发送命令
            await _mediator.Send(command);
        }
    }
}
