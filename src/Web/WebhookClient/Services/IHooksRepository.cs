namespace WebhookClient.Services;

/// <summary>
/// 定义用于管理 WebHook 数据的存储库接口
/// </summary>
public interface IHooksRepository
{
    /// <summary>
    /// 异步获取所有存储的 WebHook 接收记录
    /// </summary>
    /// <returns>WebHook 接收记录的集合</returns>
    Task<IEnumerable<WebHookReceived>> GetAll();

    /// <summary>
    /// 异步添加新的 WebHook 接收记录到存储库
    /// </summary>
    /// <param name="hook">要添加的 WebHook 接收记录</param>
    /// <returns>表示异步操作的任务</returns>
    Task AddNew(WebHookReceived hook);
}
