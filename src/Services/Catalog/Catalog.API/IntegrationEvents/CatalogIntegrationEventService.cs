namespace Microsoft.eShopOnContainers.Services.Catalog.API.IntegrationEvents;

/// <summary>
/// 实现集成事件服务，用于在 Catalog 服务中处理集成事件日志以及事件总线的交互
/// </summary>
public class CatalogIntegrationEventService : ICatalogIntegrationEventService, IDisposable
{
    // 工厂方法，用于根据数据库连接获取集成事件日志服务实例
    private readonly Func<DbConnection, IIntegrationEventLogService> _integrationEventLogServiceFactory;
    // 事件总线接口，用于发布集成事件
    private readonly IEventBus _eventBus;
    // 数据上下文，用于操作 Catalog 数据库
    private readonly CatalogContext _catalogContext;
    // 集成事件日志服务实例，用于记录和更新事件日志状态
    private readonly IIntegrationEventLogService _eventLogService;
    // 日志记录器
    private readonly ILogger<CatalogIntegrationEventService> _logger;
    // 标记对象是否已被释放
    private volatile bool disposedValue;

    /// <summary>
    /// 构造函数，注入依赖项并通过工厂方法创建事件日志服务实例
    /// </summary>
    /// <param name="logger">日志记录器</param>
    /// <param name="eventBus">事件总线接口</param>
    /// <param name="catalogContext">数据上下文</param>
    /// <param name="integrationEventLogServiceFactory">集成事件日志服务工厂</param>
    public CatalogIntegrationEventService(
        ILogger<CatalogIntegrationEventService> logger,
        IEventBus eventBus,
        CatalogContext catalogContext,
        Func<DbConnection, IIntegrationEventLogService> integrationEventLogServiceFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _catalogContext = catalogContext ?? throw new ArgumentNullException(nameof(catalogContext));
        _integrationEventLogServiceFactory = integrationEventLogServiceFactory ?? throw new ArgumentNullException(nameof(integrationEventLogServiceFactory));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        // 根据当前数据库连接创建事件日志服务实例
        _eventLogService = _integrationEventLogServiceFactory(_catalogContext.Database.GetDbConnection());
    }

    /// <summary>
    /// 通过事件总线发布集成事件，并更新事件日志状态
    /// </summary>
    /// <param name="evt">集成事件对象</param>
    public async Task PublishThroughEventBusAsync(IntegrationEvent evt)
    {
        try
        {
            // 记录信息：即将发布集成事件
            _logger.LogInformation("----- Publishing integration event: {IntegrationEventId_published} from {AppName} - ({@IntegrationEvent})", evt.Id, Program.AppName, evt);

            // 标记事件为正在处理中
            await _eventLogService.MarkEventAsInProgressAsync(evt.Id);
            // 通过事件总线发布事件
            _eventBus.Publish(evt);
            // 标记事件为已发布
            await _eventLogService.MarkEventAsPublishedAsync(evt.Id);
        }
        catch (Exception ex)
        {
            // 记录错误并将事件状态更新为发布失败
            _logger.LogError(ex, "ERROR Publishing integration event: {IntegrationEventId} from {AppName} - ({@IntegrationEvent})", evt.Id, Program.AppName, evt);
            await _eventLogService.MarkEventAsFailedAsync(evt.Id);
        }
    }

    /// <summary>
    /// 保存 Catalog 数据上下文的更改以及集成事件日志条目，确保操作原子性
    /// </summary>
    /// <param name="evt">集成事件对象</param>
    public async Task SaveEventAndCatalogContextChangesAsync(IntegrationEvent evt)
    {
        _logger.LogInformation("----- CatalogIntegrationEventService - Saving changes and integrationEvent: {IntegrationEventId}", evt.Id);

        // 使用 EF Core 的重试策略，确保同时处理多个 DbContext 下的事务操作
        await ResilientTransaction.New(_catalogContext).ExecuteAsync(async () =>
        {
            // 原子性操作：先保存 Catalog 数据库更改
            await _catalogContext.SaveChangesAsync();
            // 保存集成事件日志，同时关联当前数据库事务
            await _eventLogService.SaveEventAsync(evt, _catalogContext.Database.CurrentTransaction);
        });
    }

    /// <summary>
    /// 释放托管资源
    /// </summary>
    /// <param name="disposing">指示是否释放托管资源</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // 如果事件日志服务实现了 IDisposable，调用其 Dispose 方法
                (_eventLogService as IDisposable)?.Dispose();
            }

            disposedValue = true;
        }
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        // 防止终结器重复释放资源
        GC.SuppressFinalize(this);
    }
}
