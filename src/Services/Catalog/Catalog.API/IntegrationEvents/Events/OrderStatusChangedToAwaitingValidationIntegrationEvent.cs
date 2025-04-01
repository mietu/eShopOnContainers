namespace Microsoft.eShopOnContainers.Services.Catalog.API.IntegrationEvents.Events;

// 表示订单状态变更为待验证时的集成事件，继承自 IntegrationEvent 基类
public record OrderStatusChangedToAwaitingValidationIntegrationEvent : IntegrationEvent
{
    // 订单唯一标识
    public int OrderId { get; }

    // 订单中包含的库存项集合
    public IEnumerable<OrderStockItem> OrderStockItems { get; }

    // 构造函数，初始化订单ID和库存项集合
    public OrderStatusChangedToAwaitingValidationIntegrationEvent(int orderId,
        IEnumerable<OrderStockItem> orderStockItems)
    {
        OrderId = orderId;
        OrderStockItems = orderStockItems;
    }
}

// 表示订单中的单个库存项
public record OrderStockItem
{
    // 产品编号
    public int ProductId { get; }

    // 购买的产品数量
    public int Units { get; }

    // 构造函数，初始化产品编号和数量
    public OrderStockItem(int productId, int units)
    {
        ProductId = productId;
        Units = units;
    }
}
