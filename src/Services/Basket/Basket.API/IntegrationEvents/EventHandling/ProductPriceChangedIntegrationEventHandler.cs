namespace Microsoft.eShopOnContainers.Services.Basket.API.IntegrationEvents.EventHandling;

/// <summary>
/// 产品价格变动集成事件的处理器，用于对购物篮中的相应商品价格进行更新
/// </summary>
public class ProductPriceChangedIntegrationEventHandler : IIntegrationEventHandler<ProductPriceChangedIntegrationEvent>
{
    // 日志记录器，用于输出调试及信息日志
    private readonly ILogger<ProductPriceChangedIntegrationEventHandler> _logger;
    // 购物篮仓储接口，用于访问和更新购物篮数据
    private readonly IBasketRepository _repository;

    /// <summary>
    /// 构造函数，初始化事件处理器并注入必要的依赖
    /// </summary>
    /// <param name="logger">日志记录器</param>
    /// <param name="repository">购物篮数据仓储</param>
    public ProductPriceChangedIntegrationEventHandler(
        ILogger<ProductPriceChangedIntegrationEventHandler> logger,
        IBasketRepository repository)
    {
        // 检查依赖项是否为空，若为空则抛出异常
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <summary>
    /// 处理产品价格变动的集成事件
    /// </summary>
    /// <param name="event">产品价格变动事件参数，包含产品ID、新旧价格等信息</param>
    public async Task Handle(ProductPriceChangedIntegrationEvent @event)
    {
        // 使用LogContext添加额外属性，便于跨多个日志记录关联同一事件的上下文
        using (LogContext.PushProperty("IntegrationEventContext", $"{@event.Id}-{Program.AppName}"))
        {
            // 记录处理事件开始的信息
            _logger.LogInformation("----- Handling integration event: {IntegrationEventId} at {AppName} - ({@IntegrationEvent})",
                @event.Id, Program.AppName, @event);

            // 从仓储获取所有用户ID
            var userIds = _repository.GetUsers();

            // 针对每个用户购物篮进行更新处理
            foreach (var id in userIds)
            {
                // 异步获取对应用户的购物篮
                var basket = await _repository.GetBasketAsync(id);

                // 调用辅助方法更新购物篮中该产品的价格
                await UpdatePriceInBasketItems(@event.ProductId, @event.NewPrice, @event.OldPrice, basket);
            }
        }
    }

    /// <summary>
    /// 更新购物篮中指定产品的价格
    /// </summary>
    /// <param name="productId">需要更新价格的产品ID</param>
    /// <param name="newPrice">新的产品价格</param>
    /// <param name="oldPrice">旧的产品价格</param>
    /// <param name="basket">用户的购物篮对象</param>
    private async Task UpdatePriceInBasketItems(int productId, decimal newPrice, decimal oldPrice, CustomerBasket basket)
    {
        // 筛选出购物篮中需要更新价格的商品项
        var itemsToUpdate = basket?.Items?.Where(x => x.ProductId == productId).ToList();

        if (itemsToUpdate != null)
        {
            // 记录更新前的日志信息
            _logger.LogInformation("----- ProductPriceChangedIntegrationEventHandler - Updating items in basket for user: {BuyerId} ({@Items})",
                basket.BuyerId, itemsToUpdate);

            // 遍历所有需要更新的商品项
            foreach (var item in itemsToUpdate)
            {
                // 更新价格前确认当前单位价格与旧价格相符
                if (item.UnitPrice == oldPrice)
                {
                    // 保存原始价格后更新为新价格
                    var originalPrice = item.UnitPrice;
                    item.UnitPrice = newPrice;
                    item.OldUnitPrice = originalPrice;
                }
            }
            // 异步更新购物篮数据
            await _repository.UpdateBasketAsync(basket);
        }
    }
}
