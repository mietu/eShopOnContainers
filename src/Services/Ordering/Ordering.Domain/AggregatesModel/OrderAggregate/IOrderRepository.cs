namespace Microsoft.eShopOnContainers.Services.Ordering.Domain.AggregatesModel.OrderAggregate;

/// <summary>
/// 订单仓储接口，定义了订单领域聚合根的持久化操作。
/// 此接口继承自 IRepository<Order>，其中 Order 表示订单领域对象。
/// </summary>
public interface IOrderRepository : IRepository<Order>
{
    /// <summary>
    /// 添加订单到仓储中。
    /// </summary>
    /// <param name="order">订单对象</param>
    /// <returns>添加后的订单对象</returns>
    Order Add(Order order);

    /// <summary>
    /// 更新已存在的订单。
    /// </summary>
    /// <param name="order">订单对象</param>
    void Update(Order order);

    /// <summary>
    /// 异步获取指定订单编号的订单。
    /// </summary>
    /// <param name="orderId">订单编号</param>
    /// <returns>任务中返回获取的订单对象</returns>
    Task<Order> GetAsync(int orderId);
}
