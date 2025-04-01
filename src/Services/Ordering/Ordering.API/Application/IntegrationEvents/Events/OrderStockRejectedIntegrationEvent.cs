namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.IntegrationEvents.Events;

// 定义订单库存不足拒绝集成事件，此事件继承自 IntegrationEvent 基类
public record OrderStockRejectedIntegrationEvent : IntegrationEvent
{
    // 订单ID，用于标识拒绝事件对应的订单
    public int OrderId { get; }

    // 订单对应的所有库存项目，包含每个商品的库存确认信息
    public List<ConfirmedOrderStockItem> OrderStockItems { get; }

    // 构造函数初始化订单ID和库存项列表
    public OrderStockRejectedIntegrationEvent(int orderId, List<ConfirmedOrderStockItem> orderStockItems)
    {
        OrderId = orderId;
        OrderStockItems = orderStockItems;
    }
}

// 定义确认订单库存项，用于记录商品ID及其库存状态
public record ConfirmedOrderStockItem
{
    // 商品ID
    public int ProductId { get; }
    // 指示该商品是否有库存
    public bool HasStock { get; }

    // 构造函数初始化商品ID和库存状态
    public ConfirmedOrderStockItem(int productId, bool hasStock)
    {
        ProductId = productId;
        HasStock = hasStock;
    }
}
