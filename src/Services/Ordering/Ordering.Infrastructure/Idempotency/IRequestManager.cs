namespace Microsoft.eShopOnContainers.Services.Ordering.Infrastructure.Idempotency;

/// <summary>
/// 定义请求管理接口，用于实现请求幂等性，确保相同请求只被处理一次。
/// </summary>
public interface IRequestManager
{
    /// <summary>
    /// 检查传入的唯一标识符是否已经存在，以确认请求是否已经处理过。
    /// </summary>
    /// <param name="id">一个全局唯一的请求标识符。</param>
    /// <returns>返回一个 Task，该 Task 的结果为 true 表示该请求已经存在，false 表示不存在。</returns>
    Task<bool> ExistAsync(Guid id);

    /// <summary>
    /// 为特定命令创建请求记录，用于后续的幂等性校验。此记录确保相同命令不会被多次处理。
    /// </summary>
    /// <typeparam name="T">命令的类型。</typeparam>
    /// <param name="id">一个全局唯一的命令标识符。</param>
    /// <returns>返回一个 Task，表示异步创建记录的操作。</returns>
    Task CreateRequestForCommandAsync<T>(Guid id);
}
