namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.IntegrationEvents.Events;

// 表示订单状态变更为等待验证的集成事件，继承自 IntegrationEvent
public record OrderStatusChangedToAwaitingValidationIntegrationEvent : IntegrationEvent
{
    // 订单编号
    public int OrderId { get; }
    // 当前订单状态
    public string OrderStatus { get; }
    // 买家名称
    public string BuyerName { get; }
    // 订单库存项集合，包含订单中各个产品及其数量
    public IEnumerable<OrderStockItem> OrderStockItems { get; }

    // 构造函数，初始化订单集成事件的各个属性
    public OrderStatusChangedToAwaitingValidationIntegrationEvent(int orderId, string orderStatus, string buyerName,
        IEnumerable<OrderStockItem> orderStockItems)
    {
        OrderId = orderId;
        // 初始化订单库存项集合
        OrderStockItems = orderStockItems;
        OrderStatus = orderStatus;
        BuyerName = buyerName;
    }
}

// 表示订单中单个库存项目的记录
public record OrderStockItem
{
    // 产品编号
    public int ProductId { get; }
    // 购买的产品数量
    public int Units { get; }

    // 构造函数，初始化库存项
    public OrderStockItem(int productId, int units)
    {
        ProductId = productId;
        Units = units;
    }
}
