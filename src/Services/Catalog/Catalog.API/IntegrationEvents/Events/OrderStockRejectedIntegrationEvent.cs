namespace Microsoft.eShopOnContainers.Services.Catalog.API.IntegrationEvents.Events;

/// <summary>
/// 订单库存拒绝事件，表示因库存不足导致订单处理失败
/// 此事件继承自 IntegrationEvent 基类，包含基本事件属性（如事件ID和创建时间）
/// </summary>
public record OrderStockRejectedIntegrationEvent : IntegrationEvent
{
    /// <summary>
    /// 订单ID，用于标识发生库存拒绝的订单
    /// </summary>
    public int OrderId { get; }

    /// <summary>
    /// 包含订单中每个商品库存确认状态的列表
    /// </summary>
    public List<ConfirmedOrderStockItem> OrderStockItems { get; }

    /// <summary>
    /// 构造函数：初始化订单库存拒绝事件
    /// </summary>
    /// <param name="orderId">订单ID</param>
    /// <param name="orderStockItems">包含每个商品库存状态的列表</param>
    public OrderStockRejectedIntegrationEvent(int orderId,
        List<ConfirmedOrderStockItem> orderStockItems)
    {
        OrderId = orderId;
        OrderStockItems = orderStockItems;
    }
}

/// <summary>
/// 确认订单库存项记录，记录单个商品的库存状态信息
/// </summary>
public record ConfirmedOrderStockItem
{
    /// <summary>
    /// 商品ID，用于标识具体的商品
    /// </summary>
    public int ProductId { get; }

    /// <summary>
    /// 指示该商品是否有库存（true表示有库存，false表示无库存）
    /// </summary>
    public bool HasStock { get; }

    /// <summary>
    /// 构造函数：初始化单个商品的库存状态
    /// </summary>
    /// <param name="productId">商品ID</param>
    /// <param name="hasStock">库存状态</param>
    public ConfirmedOrderStockItem(int productId, bool hasStock)
    {
        ProductId = productId;
        HasStock = hasStock;
    }
}
