namespace WebhookClient.Services;

/// <summary>
/// 内存中存储WebHook数据的仓储实现
/// </summary>
public class InMemoryHooksRepository : IHooksRepository
{
    /// <summary>
    /// 用于存储WebHook数据的内存列表
    /// </summary>
    private readonly List<WebHookReceived> _data;

    /// <summary>
    /// 初始化新的<see cref="InMemoryHooksRepository"/>实例
    /// </summary>
    public InMemoryHooksRepository() => _data = new List<WebHookReceived>();

    /// <summary>
    /// 添加新的WebHook数据到仓储中
    /// </summary>
    /// <param name="hook">要添加的WebHook数据</param>
    /// <returns>表示异步操作的任务</returns>
    public Task AddNew(WebHookReceived hook)
    {
        _data.Add(hook);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 获取仓储中所有的WebHook数据
    /// </summary>
    /// <returns>包含所有WebHook数据的可枚举集合</returns>
    public Task<IEnumerable<WebHookReceived>> GetAll()
    {
        return Task.FromResult(_data.AsEnumerable());
    }
}
