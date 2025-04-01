namespace Microsoft.eShopOnContainers.Services.Ordering.Domain.AggregatesModel.BuyerAggregate;

// IBuyerRepository接口定义了针对Buyer聚合根的仓储操作
// 它继承自IRepository<Buyer>，意味着此仓储必须具备通用的单元操作以及聚合根管理功能
public interface IBuyerRepository : IRepository<Buyer>
{
    // Add方法用于添加一个Buyer实体，并返回添加后的Buyer
    Buyer Add(Buyer buyer);

    // Update方法用于更新一个Buyer实体，并返回更新后的Buyer
    Buyer Update(Buyer buyer);

    // FindAsync方法根据Buyer的IdentityGuid查找Buyer实体，返回一个异步任务
    Task<Buyer> FindAsync(string BuyerIdentityGuid);

    // FindByIdAsync方法根据Buyer的标识id查找Buyer实体，返回一个异步任务
    Task<Buyer> FindByIdAsync(string id);
}

