namespace Microsoft.eShopOnContainers.BuildingBlocks.EventBusRabbitMQ;

/// <summary>
/// 表示一个持久化的 RabbitMQ 连接接口，
/// 用于确保应用程序与 RabbitMQ 的连接能够自动重连和正常创建通信通道。
/// </summary>
public interface IRabbitMQPersistentConnection : IDisposable
{
    /// <summary>
    /// 获取当前连接是否可用的状态。
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// 尝试建立与 RabbitMQ 的连接。
    /// 返回 true 表示连接成功，否则返回 false。
    /// </summary>
    /// <returns>是否成功建立连接</returns>
    bool TryConnect();

    /// <summary>
    /// 创建并返回一个新的 RabbitMQ 信道（IModel）。
    /// 用于发送或接收消息。
    /// </summary>
    /// <returns>RabbitMQ 信道</returns>
    IModel CreateModel();
}
