namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.DomainEventHandlers.OrderStockConfirmed;

// 处理订单库存确认事件的处理器，负责更新订单状态并触发集成事件
public class OrderStatusChangedToStockConfirmedDomainEventHandler : INotificationHandler<OrderStatusChangedToStockConfirmedDomainEvent>
{
    // 订单仓储：用于查询和操作订单数据
    private readonly IOrderRepository _orderRepository;
    // 购买者仓储：用于查询购买者信息
    private readonly IBuyerRepository _buyerRepository;
    // 日志工厂：用于创建日志记录器记录日志
    private readonly ILoggerFactory _logger;
    // 集成事件服务：用于发布相关的集成事件到其他服务
    private readonly IOrderingIntegrationEventService _orderingIntegrationEventService;

    // 构造函数，注入所需要的仓储、日志和集成事件服务
    public OrderStatusChangedToStockConfirmedDomainEventHandler(
        IOrderRepository orderRepository,
        IBuyerRepository buyerRepository,
        ILoggerFactory logger,
        IOrderingIntegrationEventService orderingIntegrationEventService)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _buyerRepository = buyerRepository ?? throw new ArgumentNullException(nameof(buyerRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _orderingIntegrationEventService = orderingIntegrationEventService;
    }

    // Handle方法：处理 OrderStatusChangedToStockConfirmedDomainEvent 事件
    public async Task Handle(OrderStatusChangedToStockConfirmedDomainEvent orderStatusChangedToStockConfirmedDomainEvent, CancellationToken cancellationToken)
    {
        // 创建日志记录器，并记录订单状态更改为 StockConfirmed 的详细信息
        _logger.CreateLogger<OrderStatusChangedToStockConfirmedDomainEventHandler>()
            .LogTrace("Order with Id: {OrderId} has been successfully updated to status {Status} ({Id})",
                orderStatusChangedToStockConfirmedDomainEvent.OrderId, nameof(OrderStatus.StockConfirmed), OrderStatus.StockConfirmed.Id);

        // 根据订单ID从仓储中获取订单详细信息
        var order = await _orderRepository.GetAsync(orderStatusChangedToStockConfirmedDomainEvent.OrderId);
        // 根据订单中的BuyerId获取购买者信息（转换为字符串）
        var buyer = await _buyerRepository.FindByIdAsync(order.GetBuyerId.Value.ToString());

        // 创建新的集成事件，包含订单ID、订单状态和购买者名称
        var orderStatusChangedToStockConfirmedIntegrationEvent = new OrderStatusChangedToStockConfirmedIntegrationEvent(order.Id, order.OrderStatus.Name, buyer.Name);
        // 将集成事件添加并保存，后续发布给相关微服务
        await _orderingIntegrationEventService.AddAndSaveEventAsync(orderStatusChangedToStockConfirmedIntegrationEvent);
    }
}
