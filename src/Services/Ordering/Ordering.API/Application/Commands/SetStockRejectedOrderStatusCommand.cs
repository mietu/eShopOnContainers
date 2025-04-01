namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.Commands;

/// <summary>
/// 该命令用于设置由于库存不足而拒绝的订单状态。
/// </summary>
public class SetStockRejectedOrderStatusCommand : IRequest<bool>
{
    /// <summary>
    /// 订单号，标识唯一订单。
    /// </summary>
    [DataMember]
    public int OrderNumber { get; private set; }

    /// <summary>
    /// 订单中涉及的库存项列表（库存标识）。
    /// </summary>
    [DataMember]
    public List<int> OrderStockItems { get; private set; }

    /// <summary>
    /// 构造方法，初始化订单号和库存项列表。
    /// </summary>
    /// <param name="orderNumber">订单编号</param>
    /// <param name="orderStockItems">订单中需要检查库存的项集合</param>
    public SetStockRejectedOrderStatusCommand(int orderNumber, List<int> orderStockItems)
    {
        OrderNumber = orderNumber;
        OrderStockItems = orderStockItems;
    }
}
