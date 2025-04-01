namespace Microsoft.eShopOnContainers.Services.Catalog.API.IntegrationEvents.EventHandling;

// 处理订单状态变更为"等待验证"的集成事件处理器
public class OrderStatusChangedToAwaitingValidationIntegrationEventHandler :
    IIntegrationEventHandler<OrderStatusChangedToAwaitingValidationIntegrationEvent>
{
    // 数据库上下文，用于操作CatalogItems集合
    private readonly CatalogContext _catalogContext;
    // 集成事件服务，负责保存和发布集成事件
    private readonly ICatalogIntegrationEventService _catalogIntegrationEventService;
    // 日志记录器，用于记录事件处理日志
    private readonly ILogger<OrderStatusChangedToAwaitingValidationIntegrationEventHandler> _logger;

    // 构造函数，通过依赖注入获得所需的服务
    public OrderStatusChangedToAwaitingValidationIntegrationEventHandler(
        CatalogContext catalogContext,
        ICatalogIntegrationEventService catalogIntegrationEventService,
        ILogger<OrderStatusChangedToAwaitingValidationIntegrationEventHandler> logger)
    {
        _catalogContext = catalogContext;
        _catalogIntegrationEventService = catalogIntegrationEventService;
        // 如果logger为null则抛出ArgumentNullException
        _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
    }

    // 处理订单状态变更为等待验证的集成事件
    public async Task Handle(OrderStatusChangedToAwaitingValidationIntegrationEvent @event)
    {
        // 使用LogContext为当前日志上下文压入额外属性，用于事件追踪
        using (LogContext.PushProperty("IntegrationEventContext", $"{@event.Id}-{Program.AppName}"))
        {
            // 记录日志，标记开始处理集成事件
            _logger.LogInformation("----- Handling integration event: {IntegrationEventId} at {AppName} - ({@IntegrationEvent})",
                @event.Id, Program.AppName, @event);

            // 用于存放每个订单库存项目是否确认库存充足的确认结果
            var confirmedOrderStockItems = new List<ConfirmedOrderStockItem>();

            // 遍历订单中的每个库存项
            foreach (var orderStockItem in @event.OrderStockItems)
            {
                // 从数据库中查找对应的CatalogItem（产品）
                var catalogItem = _catalogContext.CatalogItems.Find(orderStockItem.ProductId);
                // 判断产品是否有足够的库存满足订单需求
                var hasStock = catalogItem.AvailableStock >= orderStockItem.Units;
                // 创建一个确认对象，标志该产品是否有足够库存
                var confirmedOrderStockItem = new ConfirmedOrderStockItem(catalogItem.Id, hasStock);

                // 将确认结果添加到列表中
                confirmedOrderStockItems.Add(confirmedOrderStockItem);
            }

            // 根据是否存在库存不足的情况确定发布的集成事件类型
            var confirmedIntegrationEvent = confirmedOrderStockItems.Any(c => !c.HasStock)
                // 如果有库存不足的项目，则使用OrderStockRejectedIntegrationEvent事件
                ? (IntegrationEvent)new OrderStockRejectedIntegrationEvent(@event.OrderId, confirmedOrderStockItems)
                // 否则，使用OrderStockConfirmedIntegrationEvent事件
                : new OrderStockConfirmedIntegrationEvent(@event.OrderId);

            // 保存事件和数据库上下文的变更
            await _catalogIntegrationEventService.SaveEventAndCatalogContextChangesAsync(confirmedIntegrationEvent);
            // 通过事件总线发布集成事件
            await _catalogIntegrationEventService.PublishThroughEventBusAsync(confirmedIntegrationEvent);
        }
    }
}
