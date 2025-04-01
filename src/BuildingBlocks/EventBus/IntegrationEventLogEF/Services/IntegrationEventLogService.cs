namespace Microsoft.eShopOnContainers.BuildingBlocks.IntegrationEventLogEF.Services;

// 解析并注释后的 IntegrationEventLogService 类
public class IntegrationEventLogService : IIntegrationEventLogService, IDisposable
{
    // 事件日志上下文，用于操作 IntegrationEventLogs 数据表
    private readonly IntegrationEventLogContext _integrationEventLogContext;
    // 数据库连接对象
    private readonly DbConnection _dbConnection;
    // 存储从程序集中检索到的所有 IntegrationEvent 类型
    private readonly List<Type> _eventTypes;
    // 用于标识当前对象是否已被释放
    private volatile bool _disposedValue;

    // 构造函数，接收数据库连接对象
    public IntegrationEventLogService(DbConnection dbConnection)
    {
        _dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
        // 使用传入的数据库连接配置并实例化上下文
        _integrationEventLogContext = new IntegrationEventLogContext(
            new DbContextOptionsBuilder<IntegrationEventLogContext>()
                .UseSqlServer(_dbConnection)
                .Options);

        // 从当前程序集中查找名称以 'IntegrationEvent' 结尾的所有类型
        _eventTypes = Assembly.Load(Assembly.GetEntryAssembly().FullName)
            .GetTypes()
            .Where(t => t.Name.EndsWith(nameof(IntegrationEvent)))
            .ToList();
    }

    // 获取指定事务ID下所有未发布的事件日志记录
    public async Task<IEnumerable<IntegrationEventLogEntry>> RetrieveEventLogsPendingToPublishAsync(Guid transactionId)
    {
        var tid = transactionId.ToString();

        // 查询出匹配的日志记录（事务ID匹配且状态为 NotPublished）
        var result = await _integrationEventLogContext.IntegrationEventLogs
            .Where(e => e.TransactionId == tid && e.State == EventStateEnum.NotPublished)
            .ToListAsync();

        // 如果有结果，则将结果按创建时间排序，并反序列化事件内容
        if (result.Any())
        {
            return result
                .OrderBy(o => o.CreationTime)
                .Select(e => e.DeserializeJsonContent(_eventTypes.Find(t => t.Name == e.EventTypeShortName)));
        }

        return new List<IntegrationEventLogEntry>();
    }

    // 保存事件日志记录，并与外部事务绑定
    public Task SaveEventAsync(IntegrationEvent @event, IDbContextTransaction transaction)
    {
        if (transaction == null) throw new ArgumentNullException(nameof(transaction));

        // 根据传入的集成事件和事务ID创建日志条目
        var eventLogEntry = new IntegrationEventLogEntry(@event, transaction.TransactionId);

        // 在当前上下文中使用外部事务
        _integrationEventLogContext.Database.UseTransaction(transaction.GetDbTransaction());
        // 将新日志条目添加到上下文
        _integrationEventLogContext.IntegrationEventLogs.Add(eventLogEntry);

        // 保存更改
        return _integrationEventLogContext.SaveChangesAsync();
    }

    // 将事件标记为已发布
    public Task MarkEventAsPublishedAsync(Guid eventId)
    {
        return UpdateEventStatus(eventId, EventStateEnum.Published);
    }

    // 将事件标记为进行中
    public Task MarkEventAsInProgressAsync(Guid eventId)
    {
        return UpdateEventStatus(eventId, EventStateEnum.InProgress);
    }

    // 将事件标记为发布失败
    public Task MarkEventAsFailedAsync(Guid eventId)
    {
        return UpdateEventStatus(eventId, EventStateEnum.PublishedFailed);
    }

    // 更新事件状态的内部方法
    private Task UpdateEventStatus(Guid eventId, EventStateEnum status)
    {
        // 从数据库检索出目标事件日志条目
        var eventLogEntry = _integrationEventLogContext.IntegrationEventLogs
            .Single(ie => ie.EventId == eventId);

        // 设置新状态
        eventLogEntry.State = status;

        // 如果状态为 InProgress，则次数加一
        if (status == EventStateEnum.InProgress)
            eventLogEntry.TimesSent++;

        // 更新实体并保存
        _integrationEventLogContext.IntegrationEventLogs.Update(eventLogEntry);
        return _integrationEventLogContext.SaveChangesAsync();
    }

    // 释放托管资源的保护方法
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // 释放上下文
                _integrationEventLogContext?.Dispose();
            }
            _disposedValue = true;
        }
    }

    // 实现IDisposable接口，供外部调用释放资源
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
