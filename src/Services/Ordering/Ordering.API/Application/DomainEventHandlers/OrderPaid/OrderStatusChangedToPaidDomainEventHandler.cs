namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.DomainEventHandlers.OrderPaid;

// 处理订单支付后状态改变的领域事件
public class OrderStatusChangedToPaidDomainEventHandler : INotificationHandler<OrderStatusChangedToPaidDomainEvent>
{
    // 注入订单仓储接口，用于获取订单数据
    private readonly IOrderRepository _orderRepository;
    // 注入日志工厂，用于创建日志记录器
    private readonly ILoggerFactory _logger;
    // 注入买家仓储接口，用于获取买家数据
    private readonly IBuyerRepository _buyerRepository;
    // 注入集成事件服务，用于处理集成事件的存储和发布
    private readonly IOrderingIntegrationEventService _orderingIntegrationEventService;

    // 构造函数注入依赖服务，确保不为空
    public OrderStatusChangedToPaidDomainEventHandler(
        IOrderRepository orderRepository,
        ILoggerFactory logger,
        IBuyerRepository buyerRepository,
        IOrderingIntegrationEventService orderingIntegrationEventService)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _buyerRepository = buyerRepository ?? throw new ArgumentNullException(nameof(buyerRepository));
        _orderingIntegrationEventService = orderingIntegrationEventService ?? throw new ArgumentNullException(nameof(orderingIntegrationEventService));
    }

    // 处理 OrderStatusChangedToPaidDomainEvent 领域事件的异步方法
    public async Task Handle(OrderStatusChangedToPaidDomainEvent orderStatusChangedToPaidDomainEvent, CancellationToken cancellationToken)
    {
        // 记录日志：订单状态已更改为支付成功
        _logger.CreateLogger<OrderStatusChangedToPaidDomainEventHandler>()
            .LogTrace("Order with Id: {OrderId} has been successfully updated to status {Status} ({Id})",
                orderStatusChangedToPaidDomainEvent.OrderId, nameof(OrderStatus.Paid), OrderStatus.Paid.Id);

        // 从订单仓储中获取订单
        var order = await _orderRepository.GetAsync(orderStatusChangedToPaidDomainEvent.OrderId);
        // 根据订单中的买家标识获取买家信息
        var buyer = await _buyerRepository.FindByIdAsync(order.GetBuyerId.Value.ToString());

        // 将订单中的每个订单项转换为 OrderStockItem，便于后续库存处理
        var orderStockList = orderStatusChangedToPaidDomainEvent.OrderItems
            .Select(orderItem => new OrderStockItem(orderItem.ProductId, orderItem.GetUnits()));

        // 创建订单状态更改后的集成事件，包含订单Id、订单状态、买家名称及相关订单项信息
        var orderStatusChangedToPaidIntegrationEvent = new OrderStatusChangedToPaidIntegrationEvent(
            orderStatusChangedToPaidDomainEvent.OrderId,
            order.OrderStatus.Name,
            buyer.Name,
            orderStockList);

        // 将集成事件添加到事件服务并保存，以便后续发布处理
        await _orderingIntegrationEventService.AddAndSaveEventAsync(orderStatusChangedToPaidIntegrationEvent);
    }
}
