namespace Microsoft.eShopOnContainers.Services.Catalog.API.IntegrationEvents.EventHandling;

/// <summary>
/// 处理订单状态变为 "Paid" 的集成事件，更新目录库存信息
/// </summary>
public class OrderStatusChangedToPaidIntegrationEventHandler : IIntegrationEventHandler<OrderStatusChangedToPaidIntegrationEvent>
{
    // 注入 CatalogContext 用以访问数据库中的目录项
    private readonly CatalogContext _catalogContext;
    // 注入 ILogger 用以记录日志信息
    private readonly ILogger<OrderStatusChangedToPaidIntegrationEventHandler> _logger;

    /// <summary>
    /// 构造函数，初始化所需依赖项
    /// </summary>
    /// <param name="catalogContext">数据库上下文，用于操作CatalogItems</param>
    /// <param name="logger">用于记录日志</param>
    public OrderStatusChangedToPaidIntegrationEventHandler(
        CatalogContext catalogContext,
        ILogger<OrderStatusChangedToPaidIntegrationEventHandler> logger)
    {
        _catalogContext = catalogContext;
        // 当 logger 为 null 时抛出异常
        _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 处理 OrderStatusChangedToPaidIntegrationEvent 事件，更新库存状态
    /// </summary>
    /// <param name="event">包含订单信息及库存变更列表的事件</param>
    public async Task Handle(OrderStatusChangedToPaidIntegrationEvent @event)
    {
        // 使用 LogContext 将 IntegrationEventContext 属性添加到日志中，用于追踪日志上下文
        using (LogContext.PushProperty("IntegrationEventContext", $"{@event.Id}-{Program.AppName}"))
        {
            // 记录处理集成事件的开始信息
            _logger.LogInformation("----- Handling integration event: {IntegrationEventId} at {AppName} - ({@IntegrationEvent})",
                @event.Id, Program.AppName, @event);

            // 遍历订单中每个库存项
            foreach (var orderStockItem in @event.OrderStockItems)
            {
                // 根据产品ID查找对应的目录项
                var catalogItem = _catalogContext.CatalogItems.Find(orderStockItem.ProductId);

                // 从库存中移除相应数量
                catalogItem.RemoveStock(orderStockItem.Units);
            }

            // 将所有更改保存到数据库
            await _catalogContext.SaveChangesAsync();
        }
    }
}
