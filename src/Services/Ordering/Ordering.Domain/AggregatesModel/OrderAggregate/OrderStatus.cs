using Microsoft.eShopOnContainers.Services.Ordering.Domain.SeedWork;

namespace Microsoft.eShopOnContainers.Services.Ordering.Domain.AggregatesModel.OrderAggregate;

/// <summary>
/// OrderStatus 类：表示订单状态，继承自 Enumeration，用于管理订单不同的状态。
/// Enumeration 是一个基类，用于定义带有有限集合的可枚举对象，并提供基础方法。
/// </summary>
public class OrderStatus : Enumeration
{
    // 定义订单状态的静态实例，每个状态用唯一的 id 和名称表示
    public static OrderStatus Submitted = new OrderStatus(1, nameof(Submitted).ToLowerInvariant());
    public static OrderStatus AwaitingValidation = new OrderStatus(2, nameof(AwaitingValidation).ToLowerInvariant());
    public static OrderStatus StockConfirmed = new OrderStatus(3, nameof(StockConfirmed).ToLowerInvariant());
    public static OrderStatus Paid = new OrderStatus(4, nameof(Paid).ToLowerInvariant());
    public static OrderStatus Shipped = new OrderStatus(5, nameof(Shipped).ToLowerInvariant());
    public static OrderStatus Cancelled = new OrderStatus(6, nameof(Cancelled).ToLowerInvariant());

    /// <summary>
    /// 构造函数：传入 id 和名称，调用基类构造函数初始化
    /// </summary>
    /// <param name="id">状态的唯一标识</param>
    /// <param name="name">状态名称</param>
    public OrderStatus(int id, string name)
        : base(id, name)
    {
    }

    /// <summary>
    /// 返回所有订单状态的集合
    /// </summary>
    /// <returns>所有订单状态的 IEnumerable 集合</returns>
    public static IEnumerable<OrderStatus> List() =>
        new[] { Submitted, AwaitingValidation, StockConfirmed, Paid, Shipped, Cancelled };

    /// <summary>
    /// 根据提供的名称查找对应的订单状态
    /// </summary>
    /// <param name="name">状态名称（大小写不敏感）</param>
    /// <returns>匹配的 OrderStatus 实例</returns>
    /// <exception cref="OrderingDomainException">如果找不到匹配的状态, 则抛出异常，列出所有可能的值</exception>
    public static OrderStatus FromName(string name)
    {
        // 在所有状态中查找名称匹配的订单状态（不区分大小写）
        var state = List()
            .SingleOrDefault(s => string.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));

        if (state == null)
        {
            // 抛出异常并包括所有可能的状态名作为提示
            throw new OrderingDomainException($"Possible values for OrderStatus: {string.Join(",", List().Select(s => s.Name))}");
        }

        return state;
    }

    /// <summary>
    /// 根据提供的 id 查找对应的订单状态
    /// </summary>
    /// <param name="id">状态的唯一标识</param>
    /// <returns>匹配的 OrderStatus 实例</returns>
    /// <exception cref="OrderingDomainException">如果找不到匹配的状态, 则抛出异常，列出所有可能的值</exception>
    public static OrderStatus From(int id)
    {
        // 在所有状态中查找 id 匹配的订单状态
        var state = List().SingleOrDefault(s => s.Id == id);

        if (state == null)
        {
            // 抛出异常并包括所有可能的状态名作为提示
            throw new OrderingDomainException($"Possible values for OrderStatus: {string.Join(",", List().Select(s => s.Name))}");
        }

        return state;
    }
}
