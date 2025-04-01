namespace Microsoft.eShopOnContainers.BuildingBlocks.IntegrationEventLogEF.Utilities;

// ResilientTransaction 用于多 DbContext 事务时提供弹性重试策略
public class ResilientTransaction
{
    private readonly DbContext _context;

    // 构造函数，确保传入的 DbContext 非空
    private ResilientTransaction(DbContext context) =>
        _context = context ?? throw new ArgumentNullException(nameof(context));

    // 创建 ResilientTransaction 静态工厂方法
    public static ResilientTransaction New(DbContext context) => new(context);

    // 执行带有 EF Core 弹性策略的事务
    public async Task ExecuteAsync(Func<Task> action)
    {
        // 获取数据库执行策略（用于处理瞬时故障）
        var strategy = _context.Database.CreateExecutionStrategy();

        // 利用策略执行，若失败可在策略内部重试
        await strategy.ExecuteAsync(async () =>
        {
            // 显式启用事务
            await using var transaction = await _context.Database.BeginTransactionAsync();
            // 执行调用方提供的异步委托
            await action();
            // 提交事务
            await transaction.CommitAsync();
        });
    }
}
