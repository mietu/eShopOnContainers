namespace Microsoft.eShopOnContainers.Services.Ordering.Domain.Seedwork;

// 定义一个工作单元接口，用于管理实体的持久化操作以及领域事件的一致性提交
public interface IUnitOfWork : IDisposable
{
    // 异步保存所有变更，并返回受影响的记录数
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));

    // 异步保存实体的更改，同时也会处理领域事件，返回操作是否成功
    Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default(CancellationToken));
}
