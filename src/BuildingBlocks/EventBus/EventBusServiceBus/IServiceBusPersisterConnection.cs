namespace Microsoft.eShopOnContainers.BuildingBlocks.EventBusServiceBus;

// 此接口定义了 Service Bus 持久连接的约定，
// 用于确保在与 Azure Service Bus 交互时能够使用预配置的客户端实例进行操作。
// 它继承自 IAsyncDisposable 接口，保证可以进行异步资源释放。
public interface IServiceBusPersisterConnection : IAsyncDisposable
{
    // TopicClient 属性用于获取 Service Bus 主题的客户端实例，
    // 该客户端封装了与 Service Bus 主题通信的功能。
    ServiceBusClient TopicClient { get; }

    // AdministrationClient 属性用于获取用于管理 Service Bus 资源的客户端实例，
    // 例如队列、主题和订阅等管理操作。
    ServiceBusAdministrationClient AdministrationClient { get; }
}
