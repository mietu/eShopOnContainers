namespace Microsoft.eShopOnContainers.BuildingBlocks.IntegrationEventLogEF.Services;

/// <summary>
/// 用于管理集成事件日志的服务接口
/// </summary>
public interface IIntegrationEventLogService
{
    /// <summary>
    /// 获取指定事务ID下待发布的所有事件日志条目
    /// </summary>
    /// <param name="transactionId">事务ID</param>
    /// <returns>返回所有待发布的事件日志条目</returns>
    Task<IEnumerable<IntegrationEventLogEntry>> RetrieveEventLogsPendingToPublishAsync(Guid transactionId);

    /// <summary>
    /// 将新的集成事件和事务上下文一起保存到事件日志中
    /// </summary>
    /// <param name="event">集成事件对象</param>
    /// <param name="transaction">事务对象</param>
    Task SaveEventAsync(IntegrationEvent @event, IDbContextTransaction transaction);

    /// <summary>
    /// 更新事件状态为“已发布”
    /// </summary>
    /// <param name="eventId">事件ID</param>
    Task MarkEventAsPublishedAsync(Guid eventId);

    /// <summary>
    /// 更新事件状态为“进行中”
    /// </summary>
    /// <param name="eventId">事件ID</param>
    Task MarkEventAsInProgressAsync(Guid eventId);

    /// <summary>
    /// 更新事件状态为“发布失败”
    /// </summary>
    /// <param name="eventId">事件ID</param>
    Task MarkEventAsFailedAsync(Guid eventId);
}
