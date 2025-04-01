namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.IntegrationEvents.Events;

// 定义一个不可变 (record) 类型，继承自 IntegrationEvent 基类
public record OrderStatusChangedToPaidIntegrationEvent : IntegrationEvent
{
    // 订单ID，标识该订单
    public int OrderId { get; }

    // 订单状态，例如 "Paid"，描述当前订单的状态
    public string OrderStatus { get; }

    // 买家名称，用于标识订单的购买人
    public string BuyerName { get; }

    // 订单中的库存项集合，每个库存项包含产品ID及数量
    public IEnumerable<OrderStockItem> OrderStockItems { get; }

    // 构造函数，初始化 OrderStatusChangedToPaidIntegrationEvent 实例
    // 参数：
    // orderId - 订单ID
    // orderStatus - 订单当前状态描述（例如“Paid”）
    // buyerName - 买家的名称
    // orderStockItems - 当前订单中包含的库存产品集合
    public OrderStatusChangedToPaidIntegrationEvent(int orderId,
        string orderStatus,
        string buyerName,
        IEnumerable<OrderStockItem> orderStockItems)
    {
        OrderId = orderId;
        OrderStockItems = orderStockItems;
        OrderStatus = orderStatus;
        BuyerName = buyerName;
    }
}

