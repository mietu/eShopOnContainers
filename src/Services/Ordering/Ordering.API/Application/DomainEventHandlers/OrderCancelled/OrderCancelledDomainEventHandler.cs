namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.DomainEventHandlers.OrderCancelled;

// 该类处理 OrderCancelledDomainEvent 领域事件，
// 实现 INotificationHandler<OrderCancelledDomainEvent> 接口
public class OrderCancelledDomainEventHandler : INotificationHandler<OrderCancelledDomainEvent>
{
    // 注入必要的仓储和服务
    private readonly IOrderRepository _orderRepository;
    private readonly IBuyerRepository _buyerRepository;
    private readonly ILoggerFactory _logger;
    private readonly IOrderingIntegrationEventService _orderingIntegrationEventService;

    // 构造函数注入依赖项，并进行空检查
    public OrderCancelledDomainEventHandler(
        IOrderRepository orderRepository,
        ILoggerFactory logger,
        IBuyerRepository buyerRepository,
        IOrderingIntegrationEventService orderingIntegrationEventService)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _buyerRepository = buyerRepository ?? throw new ArgumentNullException(nameof(buyerRepository));
        _orderingIntegrationEventService = orderingIntegrationEventService;
    }

    // Handle 方法用于处理订单取消的领域事件
    public async Task Handle(OrderCancelledDomainEvent orderCancelledDomainEvent, CancellationToken cancellationToken)
    {
        // 使用 ILoggerFactory 创建记录器，并记录订单状态变更的跟踪信息
        _logger.CreateLogger<OrderCancelledDomainEvent>()
            .LogTrace("Order with Id: {OrderId} has been successfully updated to status {Status} ({Id})",
                orderCancelledDomainEvent.Order.Id, nameof(OrderStatus.Cancelled), OrderStatus.Cancelled.Id);

        // 根据事件中订单Id获取订单详细信息
        var order = await _orderRepository.GetAsync(orderCancelledDomainEvent.Order.Id);
        // 根据订单中的 BuyerId 获取买家信息（将 BuyerId 转换为字符串）
        var buyer = await _buyerRepository.FindByIdAsync(order.GetBuyerId.Value.ToString());

        // 创建集成事件，表示订单状态已更新为取消状态
        var orderStatusChangedToCancelledIntegrationEvent = new OrderStatusChangedToCancelledIntegrationEvent(order.Id, order.OrderStatus.Name, buyer.Name);
        // 将集成事件添加到事件服务以便后续保存或发布到事件总线
        await _orderingIntegrationEventService.AddAndSaveEventAsync(orderStatusChangedToCancelledIntegrationEvent);
    }
}
