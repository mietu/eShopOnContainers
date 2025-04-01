namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.DomainEventHandlers.OrderShipped;

// 处理订单已发货领域事件的处理器
public class OrderShippedDomainEventHandler : INotificationHandler<OrderShippedDomainEvent>
{
    // 订单仓储，用于查询订单数据
    private readonly IOrderRepository _orderRepository;
    // 购买者仓储，用于查询购买者数据
    private readonly IBuyerRepository _buyerRepository;
    // 集成事件服务，用于发布领域事件到集成事件总线
    private readonly IOrderingIntegrationEventService _orderingIntegrationEventService;
    // 日志工厂，用于创建日志记录器
    private readonly ILoggerFactory _logger;

    // 构造函数，通过依赖注入注入所需的服务
    public OrderShippedDomainEventHandler(
        IOrderRepository orderRepository,
        ILoggerFactory logger,
        IBuyerRepository buyerRepository,
        IOrderingIntegrationEventService orderingIntegrationEventService)
    {
        // 验证并赋值注入的依赖项
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _buyerRepository = buyerRepository ?? throw new ArgumentNullException(nameof(buyerRepository));
        _orderingIntegrationEventService = orderingIntegrationEventService;
    }

    // 当订单发货领域事件发生时执行的处理逻辑
    public async Task Handle(OrderShippedDomainEvent orderShippedDomainEvent, CancellationToken cancellationToken)
    {
        // 使用日志工厂创建针对OrderShippedDomainEvent的日志记录器，并记录一个跟踪日志
        _logger.CreateLogger<OrderShippedDomainEvent>()
            .LogTrace("Order with Id: {OrderId} has been successfully updated to status {Status} ({Id})",
                orderShippedDomainEvent.Order.Id, nameof(OrderStatus.Shipped), OrderStatus.Shipped.Id);

        // 根据领域事件中的订单ID获取最新的订单数据
        var order = await _orderRepository.GetAsync(orderShippedDomainEvent.Order.Id);
        // 根据订单中的购买者ID查询购买者信息
        // 注意：这里将购买者ID转为字符串传递给仓储查询方法
        var buyer = await _buyerRepository.FindByIdAsync(order.GetBuyerId.Value.ToString());

        // 创建订单状态变为已发货的集成事件
        var orderStatusChangedToShippedIntegrationEvent =
            new OrderStatusChangedToShippedIntegrationEvent(order.Id, order.OrderStatus.Name, buyer.Name);
        // 将集成事件添加并保存，以便后续发布到集成事件总线
        await _orderingIntegrationEventService.AddAndSaveEventAsync(orderStatusChangedToShippedIntegrationEvent);
    }
}
