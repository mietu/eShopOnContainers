namespace Microsoft.eShopOnContainers.Services.Ordering.Infrastructure.Idempotency;

// RequestManager 实现了 IRequestManager 接口，用于处理命令的幂等性问题
public class RequestManager : IRequestManager
{
    // 用于与数据库进行交互的上下文，通过依赖注入提供
    private readonly OrderingContext _context;

    // 构造函数，注入 OrderingContext，若传入 null 则抛出异常
    public RequestManager(OrderingContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    // 异步方法：检查指定 Guid 的请求记录是否存在
    public async Task<bool> ExistAsync(Guid id)
    {
        // 在 OrderingContext 中查找 ClientRequest 实例
        var request = await _context.FindAsync<ClientRequest>(id);

        // 若请求不为空则表示记录已存在，返回 true
        return request != null;
    }

    // 异步方法：为命令创建新的请求记录
    // 使用泛型参数 T 来标识命令的类型，便于在记录中保存命令名称
    public async Task CreateRequestForCommandAsync<T>(Guid id)
    {
        // 检查该请求记录是否已经存在
        var exists = await ExistAsync(id);

        // 如果该请求已存在，则直接抛出 OrderingDomainException 异常
        // 否则创建新的 ClientRequest，并赋值 Id、Name 和当前 UTC 时间
        var request = exists ?
            throw new OrderingDomainException($"Request with {id} already exists") :
            new ClientRequest()
            {
                Id = id,
                Name = typeof(T).Name,
                Time = DateTime.UtcNow
            };

        // 将新的请求记录添加到上下文
        _context.Add(request);

        // 将更改保存到数据库中
        await _context.SaveChangesAsync();
    }
}
