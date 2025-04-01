namespace Microsoft.eShopOnContainers.Services.Catalog.API.IntegrationEvents.Events;

// 定义一个记录类型 OrderStatusChangedToPaidIntegrationEvent，它继承自 IntegrationEvent 基类
// 此记录用于表示订单状态更改为“已支付”时所产生的集成事件
public record OrderStatusChangedToPaidIntegrationEvent : IntegrationEvent
{
    // 属性：订单编号（只读）
    public int OrderId { get; }

    // 属性：订单中包含的库存项目集合，记录了每个产品及对应购买数量（只读）
    public IEnumerable<OrderStockItem> OrderStockItems { get; }

    // 构造函数：初始化 OrderStatusChangedToPaidIntegrationEvent 实例
    // 参数 orderId：订单的唯一标识
    // 参数 orderStockItems：订单包含的所有库存项（产品编号及数量）
    public OrderStatusChangedToPaidIntegrationEvent(int orderId,
        IEnumerable<OrderStockItem> orderStockItems)
    {
        OrderId = orderId;                   // 设置订单编号
        OrderStockItems = orderStockItems;    // 设置相关的库存项集合
    }
}
