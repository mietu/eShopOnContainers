namespace Microsoft.eShopOnContainers.Services.Ordering.Infrastructure.Repositories;

/// <summary>
/// BuyerRepository是针对Buyer聚合根的仓储实现，负责对Buyer实体进行增删改查操作。
/// </summary>
public class BuyerRepository : IBuyerRepository
{
    // EF Core上下文，用于操作数据库中的Buyer实体和相关集合
    private readonly OrderingContext _context;

    // 单元工作模式，通过上下文实现事务管理
    public IUnitOfWork UnitOfWork => _context;

    // 构造函数，注入OrderingContext，如果传入null则抛出ArgumentNullException
    public BuyerRepository(OrderingContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    // 添加Buyer实体
    public Buyer Add(Buyer buyer)
    {
        // 如果buyer未持久化（瞬态），则将其添加到上下文中
        if (buyer.IsTransient())
        {
            return _context.Buyers
                .Add(buyer)
                .Entity;
        }

        // 若buyer已持久化，直接返回该实例
        return buyer;
    }

    // 更新Buyer实体
    public Buyer Update(Buyer buyer)
    {
        // 使用EF Core的Update方法更新buyer，并返回更新后的实体
        return _context.Buyers
                .Update(buyer)
                .Entity;
    }

    // 根据买家唯一标识（IdentityGuid）查找Buyer，包含支付方式信息
    public async Task<Buyer> FindAsync(string identity)
    {
        var buyer = await _context.Buyers
            // Include关联查询PaymentMethods集合
            .Include(b => b.PaymentMethods)
            // 根据传入的identity匹配Buyer的IdentityGuid
            .Where(b => b.IdentityGuid == identity)
            // 如果有匹配的唯一Buyer则返回，否则返回null
            .SingleOrDefaultAsync();

        return buyer;
    }

    // 根据Buyer的数据库Id查找Buyer，包含支付方式信息
    public async Task<Buyer> FindByIdAsync(string id)
    {
        var buyer = await _context.Buyers
            // Include关联查询PaymentMethods集合
            .Include(b => b.PaymentMethods)
            // 解析传入的id为整型，并查找匹配的Buyer
            .Where(b => b.Id == int.Parse(id))
            .SingleOrDefaultAsync();

        return buyer;
    }
}
