namespace Microsoft.eShopOnContainers.Services.Ordering.Infrastructure;

// OrderingContext 类继承自 DbContext 并实现 IUnitOfWork 接口，
// 用于管理订单相关的实体及其事务处理和领域事件分发。
public class OrderingContext : DbContext, IUnitOfWork
{
    // 默认数据库架构
    public const string DEFAULT_SCHEMA = "ordering";

    // 定义订单相关的 DbSet
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<PaymentMethod> Payments { get; set; }
    public DbSet<Buyer> Buyers { get; set; }
    public DbSet<CardType> CardTypes { get; set; }
    public DbSet<OrderStatus> OrderStatus { get; set; }

    // 用于分发领域事件的中介者
    private readonly IMediator _mediator;
    // 当前活动的事务对象
    private IDbContextTransaction _currentTransaction;

    // 构造函数：仅 DbContextOptions 参数，主要用于无领域事件分发场景
    public OrderingContext(DbContextOptions<OrderingContext> options) : base(options)
    {
    }

    // 获取当前事务
    public IDbContextTransaction GetCurrentTransaction() => _currentTransaction;

    // 检查是否存在活动事务
    public bool HasActiveTransaction => _currentTransaction != null;

    // 构造函数：接收 DbContextOptions 和 IMediator 参数，
    // 用于支持领域事件分发
    public OrderingContext(DbContextOptions<OrderingContext> options, IMediator mediator) : base(options)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        System.Diagnostics.Debug.WriteLine("OrderingContext::ctor ->" + this.GetHashCode());
    }

    // 配置 EF Core 模型创建，应用各个实体的配置信息
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ClientRequestEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new PaymentMethodEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new OrderEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new OrderItemEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new CardTypeEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new OrderStatusEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new BuyerEntityTypeConfiguration());
    }

    // 保存实体并分发领域事件的异步方法
    // 1. 分发所有领域事件
    // 2. 调用基类 SaveChangesAsync 将所有更改提交到数据库
    public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        // 分发领域事件，例如订单状态变化等事件
        await _mediator.DispatchDomainEventsAsync(this);

        // 提交所有 EF Core 相关的变更
        var result = await base.SaveChangesAsync(cancellationToken);

        return true;
    }

    // 开始一个新的数据库事务（如果不存在当前事务）
    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        if (_currentTransaction != null) return null;

        _currentTransaction = await Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
        return _currentTransaction;
    }

    // 提交当前事务，首先尝试保存所有变更，
    // 如果成功则提交事务，否则回滚事务并抛出异常
    public async Task CommitTransactionAsync(IDbContextTransaction transaction)
    {
        if (transaction == null)
            throw new ArgumentNullException(nameof(transaction));

        if (transaction != _currentTransaction)
            throw new InvalidOperationException($"Transaction {transaction.TransactionId} is not current");

        try
        {
            // 保存所有变更并提交
            await SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            // 如果出错，则回滚事务
            RollbackTransaction();
            throw;
        }
        finally
        {
            // 清理当前事务对象
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }

    // 回滚当前事务，如果存在的话
    public void RollbackTransaction()
    {
        try
        {
            _currentTransaction?.Rollback();
        }
        finally
        {
            // 清理事务对象
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }
}

// 设计时工厂类，用于在设计时创建 OrderingContext 实例（例如用于 EF Core CLI 工具）
public class OrderingContextDesignFactory : IDesignTimeDbContextFactory<OrderingContext>
{
    // 创建 DbContext 对象
    public OrderingContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<OrderingContext>()
            .UseSqlServer("Server=.;Initial Catalog=Microsoft.eShopOnContainers.Services.OrderingDb;Integrated Security=true");

        // 使用 NoMediator 来初始化 OrderingContext 实例
        return new OrderingContext(optionsBuilder.Options, new NoMediator());
    }

    // NoMediator 类提供一个不实际执行领域事件分发的默认实现，
    // 避免在设计时环境中引入其他依赖
    class NoMediator : IMediator
    {
        // 以下方法均返回默认值或完成任务，只用于满足接口实现要求

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            return default(IAsyncEnumerable<TResponse>);
        }

        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
        {
            return default(IAsyncEnumerable<object?>);
        }

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification
        {
            return Task.CompletedTask;
        }

        public Task Publish(object notification, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<TResponse>(default(TResponse));
        }

        public Task<object> Send(object request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(default(object));
        }
    }
}
