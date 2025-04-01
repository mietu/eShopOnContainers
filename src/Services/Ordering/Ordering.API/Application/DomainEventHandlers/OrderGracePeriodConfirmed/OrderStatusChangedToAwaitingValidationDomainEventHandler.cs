namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.DomainEventHandlers.OrderGracePeriodConfirmed;

// 处理订单状态变更到 AwaitingValidation 的领域事件通知器
public class OrderStatusChangedToAwaitingValidationDomainEventHandler
                : INotificationHandler<OrderStatusChangedToAwaitingValidationDomainEvent>
{
    // 注入依赖项：订单仓储、日志工厂、买家仓储及集成事件服务
    private readonly IOrderRepository _orderRepository;
    private readonly ILoggerFactory _logger;
    private readonly IBuyerRepository _buyerRepository;
    private readonly IOrderingIntegrationEventService _orderingIntegrationEventService;

    // 构造器，确保必需依赖项不为 null
    public OrderStatusChangedToAwaitingValidationDomainEventHandler(
        IOrderRepository orderRepository, ILoggerFactory logger,
        IBuyerRepository buyerRepository,
        IOrderingIntegrationEventService orderingIntegrationEventService)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _buyerRepository = buyerRepository;
        _orderingIntegrationEventService = orderingIntegrationEventService;
    }

    // 处理领域事件的方法
    public async Task Handle(OrderStatusChangedToAwaitingValidationDomainEvent orderStatusChangedToAwaitingValidationDomainEvent, CancellationToken cancellationToken)
    {
        // 记录一条跟踪日志，指示订单状态变更为 AwaitingValidation
        _logger.CreateLogger<OrderStatusChangedToAwaitingValidationDomainEvent>()
            .LogTrace("Order with Id: {OrderId} has been successfully updated to status {Status} ({Id})",
                orderStatusChangedToAwaitingValidationDomainEvent.OrderId, nameof(OrderStatus.AwaitingValidation), OrderStatus.AwaitingValidation.Id);

        // 根据事件中的OrderId获取订单对象
        var order = await _orderRepository.GetAsync(orderStatusChangedToAwaitingValidationDomainEvent.OrderId);

        // 根据订单中的买家Id获取买家信息
        var buyer = await _buyerRepository.FindByIdAsync(order.GetBuyerId.Value.ToString());

        // 将事件中的每个订单项映射为OrderStockItem对象，用于后续库存处理
        var orderStockList = orderStatusChangedToAwaitingValidationDomainEvent.OrderItems
            .Select(orderItem => new OrderStockItem(orderItem.ProductId, orderItem.GetUnits()));

        // 创建一个集成事件对象，包含了订单状态、买家信息及订单库存明细
        var orderStatusChangedToAwaitingValidationIntegrationEvent = new OrderStatusChangedToAwaitingValidationIntegrationEvent(
            order.Id, order.OrderStatus.Name, buyer.Name, orderStockList);

        // 将集成事件添加并保存，后续通过事件总线进行传播
        await _orderingIntegrationEventService.AddAndSaveEventAsync(orderStatusChangedToAwaitingValidationIntegrationEvent);
    }
}
