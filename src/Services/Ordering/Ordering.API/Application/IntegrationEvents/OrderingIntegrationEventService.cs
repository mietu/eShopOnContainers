namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.IntegrationEvents;

// OrderingIntegrationEventService 用于处理集成事件的保存和发布逻辑
public class OrderingIntegrationEventService : IOrderingIntegrationEventService
{
    // 用于根据 DbConnection 获取 IIntegrationEventLogService 实例的工厂方法
    private readonly Func<DbConnection, IIntegrationEventLogService> _integrationEventLogServiceFactory;
    // 用于发布事件的事件总线
    private readonly IEventBus _eventBus;
    // 数据库上下文，包含订单数据以及当前事务信息
    private readonly OrderingContext _orderingContext;
    // 记录集成事件日志的服务
    private readonly IIntegrationEventLogService _eventLogService;
    // 日志记录器，用于记录服务的操作日志
    private readonly ILogger<OrderingIntegrationEventService> _logger;

    // 构造函数注入相关依赖项，并进行空引用检查
    public OrderingIntegrationEventService(
        IEventBus eventBus,
        OrderingContext orderingContext,
        IntegrationEventLogContext eventLogContext,
        Func<DbConnection, IIntegrationEventLogService> integrationEventLogServiceFactory,
        ILogger<OrderingIntegrationEventService> logger)
    {
        // 检查 orderingContext 是否为 null
        _orderingContext = orderingContext ?? throw new ArgumentNullException(nameof(orderingContext));
        // 检查工厂方法是否为 null
        _integrationEventLogServiceFactory = integrationEventLogServiceFactory ?? throw new ArgumentNullException(nameof(integrationEventLogServiceFactory));
        // 检查事件总线是否为 null
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        // 使用工厂方法根据当前 OrderingContext 的数据库连接获取集成事件日志服务实例
        _eventLogService = _integrationEventLogServiceFactory(_orderingContext.Database.GetDbConnection());
        // 检查 logger 是否为 null
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // 发布与指定事务相关的所有待发布事件
    public async Task PublishEventsThroughEventBusAsync(Guid transactionId)
    {
        // 从事件日志服务中获取待发布的事件列表
        var pendingLogEvents = await _eventLogService.RetrieveEventLogsPendingToPublishAsync(transactionId);

        // 遍历所有待发布的日志事件
        foreach (var logEvt in pendingLogEvents)
        {
            // 记录发布事件的详细信息
            _logger.LogInformation("----- Publishing integration event: {IntegrationEventId} from {AppName} - ({@IntegrationEvent})",
                logEvt.EventId, Program.AppName, logEvt.IntegrationEvent);

            try
            {
                // 将事件状态标记为“处理中”
                await _eventLogService.MarkEventAsInProgressAsync(logEvt.EventId);
                // 通过事件总线发布事件
                _eventBus.Publish(logEvt.IntegrationEvent);
                // 发布成功后，将事件状态标记为“已发布”
                await _eventLogService.MarkEventAsPublishedAsync(logEvt.EventId);
            }
            catch (Exception ex)
            {
                // 记录发布事件时的错误信息
                _logger.LogError(ex, "ERROR publishing integration event: {IntegrationEventId} from {AppName}", logEvt.EventId, Program.AppName);
                // 出现异常后，将事件状态标记为“发布失败”
                await _eventLogService.MarkEventAsFailedAsync(logEvt.EventId);
            }
        }
    }

    // 将新的集成事件添加到事件日志，并保存到数据库事务中
    public async Task AddAndSaveEventAsync(IntegrationEvent evt)
    {
        // 记录将事件入队的操作信息
        _logger.LogInformation("----- Enqueuing integration event {IntegrationEventId} to repository ({@IntegrationEvent})", evt.Id, evt);
        // 保存事件以及其关联的当前数据库事务
        await _eventLogService.SaveEventAsync(evt, _orderingContext.GetCurrentTransaction());
    }
}
