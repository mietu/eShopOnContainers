namespace Microsoft.eShopOnContainers.BuildingBlocks.EventBusServiceBus;

/// <summary>
/// DefaultServiceBusPersisterConnection 类用于管理和维持 Service Bus 的连接。
/// </summary>
public class DefaultServiceBusPersisterConnection : IServiceBusPersisterConnection
{
    // 保存 Service Bus 连接字符串
    private readonly string _serviceBusConnectionString;
    // ServiceBusClient 实例，用于处理主题通信
    private ServiceBusClient _topicClient;
    // ServiceBusAdministrationClient 实例，用于管理订阅和其他管理操作
    private ServiceBusAdministrationClient _subscriptionClient;
    // 标识当前对象是否已处置
    bool _disposed;

    /// <summary>
    /// 构造函数，使用连接字符串初始化 Service Bus 的客户端。
    /// </summary>
    /// <param name="serviceBusConnectionString">Service Bus 连接字符串</param>
    public DefaultServiceBusPersisterConnection(string serviceBusConnectionString)
    {
        _serviceBusConnectionString = serviceBusConnectionString;
        // 初始化订阅管理客户端
        _subscriptionClient = new ServiceBusAdministrationClient(_serviceBusConnectionString);
        // 初始化主题客户端
        _topicClient = new ServiceBusClient(_serviceBusConnectionString);
    }

    /// <summary>
    /// 获取当前可用的 ServiceBusClient。如果客户端已关闭，则重新创建新的客户端。
    /// </summary>
    public ServiceBusClient TopicClient
    {
        get
        {
            // 如果当前客户端已关闭，则创建新的客户端实例
            if (_topicClient.IsClosed)
            {
                _topicClient = new ServiceBusClient(_serviceBusConnectionString);
            }
            return _topicClient;
        }
    }

    /// <summary>
    /// 提供对订阅管理客户端的访问。
    /// </summary>
    public ServiceBusAdministrationClient AdministrationClient =>
        _subscriptionClient;

    /// <summary>
    /// 创建并返回一个有效的 ServiceBusClient 实例，如果当前实例已关闭则重新创建。
    /// </summary>
    /// <returns>ServiceBusClient 实例</returns>
    public ServiceBusClient CreateModel()
    {
        // 如果当前客户端已关闭，则创建新的客户端实例
        if (_topicClient.IsClosed)
        {
            _topicClient = new ServiceBusClient(_serviceBusConnectionString);
        }

        return _topicClient;
    }

    /// <summary>
    /// 异步释放资源，确保 ServiceBusClient 被正确处置。
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        // 如果已经处置过，则直接返回
        if (_disposed) return;

        _disposed = true;
        // 释放 ServiceBusClient 资源
        await _topicClient.DisposeAsync();
    }
}
