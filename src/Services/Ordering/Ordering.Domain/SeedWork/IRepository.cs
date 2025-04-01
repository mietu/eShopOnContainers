namespace Microsoft.eShopOnContainers.Services.Ordering.Domain.Seedwork;

// IRepository 接口定义了聚合根的仓储操作，所有仓储需实现此接口
public interface IRepository<T> where T : IAggregateRoot
{
    // UnitOfWork 属性用于协调多个操作的事务提交。
    // 实现者应提供其关联的 IUnitOfWork 对象，从而在批量操作时提供一致的事务处理能力。
    IUnitOfWork UnitOfWork { get; }
}
