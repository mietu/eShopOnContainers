namespace Microsoft.eShopOnContainers.Services.Ordering.Infrastructure.Repositories;

// OrderRepository 实现了订单仓储接口 IOrderRepository
public class OrderRepository : IOrderRepository
{
    // 依赖注入 OrderingContext 用于与数据库交互
    private readonly OrderingContext _context;

    // 将 OrderingContext 同时作为 UnitOfWork 使用
    public IUnitOfWork UnitOfWork => _context;

    // 构造函数，确保传入的 OrderingContext 不为空，否则抛出异常
    public OrderRepository(OrderingContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    // 添加订单到上下文中的 Orders 集合，并返回被跟踪的订单实体
    public Order Add(Order order)
    {
        return _context.Orders.Add(order).Entity;
    }

    // 异步查询指定 OrderId 的订单
    public async Task<Order> GetAsync(int orderId)
    {
        // 通过 Include 方法加载订单关联的 Address
        var order = await _context
                            .Orders
                            .Include(x => x.Address)
                            .FirstOrDefaultAsync(o => o.Id == orderId);

        // 如果未在数据库中找到，尝试从本地跟踪的实体集合中查找
        if (order == null)
        {
            order = _context.Orders.Local.FirstOrDefault(o => o.Id == orderId);
        }

        // 如果订单存在，则异步加载关联的 OrderItems 和 OrderStatus
        if (order != null)
        {
            await _context.Entry(order)
                .Collection(i => i.OrderItems).LoadAsync();
            await _context.Entry(order)
                .Reference(i => i.OrderStatus).LoadAsync();
        }

        return order;
    }

    // 更新订单状态，将实体标识为已修改
    public void Update(Order order)
    {
        _context.Entry(order).State = EntityState.Modified;
    }
}
