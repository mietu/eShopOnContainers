namespace Microsoft.eShopOnContainers.BuildingBlocks.EventBus;

/// <summary>
/// 事件总线订阅管理器接口，用于管理事件的订阅及其处理程序注册。
/// </summary>
public interface IEventBusSubscriptionsManager
{
    // 当没有任何订阅时，返回true
    bool IsEmpty { get; }

    // 当订阅被移除时触发该事件，参数为被移除的事件名称
    event EventHandler<string> OnEventRemoved;

    /// <summary>
    /// 添加一个动态订阅。动态订阅允许运行时注册事件处理程序，
    /// 而不需要编译时确定事件类型。
    /// </summary>
    /// <typeparam name="TH">处理程序类型，必须实现IDynamicIntegrationEventHandler接口</typeparam>
    /// <param name="eventName">事件名称</param>
    void AddDynamicSubscription<TH>(string eventName)
        where TH : IDynamicIntegrationEventHandler;

    /// <summary>
    /// 添加一个静态类型的订阅，指定事件类型和对应的处理程序类型。
    /// </summary>
    /// <typeparam name="T">事件类型，必须继承自IntegrationEvent</typeparam>
    /// <typeparam name="TH">处理程序类型，必须实现IIntegrationEventHandler&lt;T&gt;接口</typeparam>
    void AddSubscription<T, TH>()
        where T : IntegrationEvent
        where TH : IIntegrationEventHandler<T>;

    /// <summary>
    /// 移除指定事件类型和处理程序类型的订阅。
    /// </summary>
    /// <typeparam name="T">事件类型</typeparam>
    /// <typeparam name="TH">处理程序类型</typeparam>
    void RemoveSubscription<T, TH>()
        where TH : IIntegrationEventHandler<T>
        where T : IntegrationEvent;

    /// <summary>
    /// 移除指定事件名称和处理程序类型的动态订阅。
    /// </summary>
    /// <typeparam name="TH">处理程序类型，必须实现IDynamicIntegrationEventHandler接口</typeparam>
    /// <param name="eventName">事件名称</param>
    void RemoveDynamicSubscription<TH>(string eventName)
        where TH : IDynamicIntegrationEventHandler;

    /// <summary>
    /// 判断是否存在该事件类型的订阅。
    /// </summary>
    /// <typeparam name="T">事件类型</typeparam>
    /// <returns>存在返回true，否则返回false</returns>
    bool HasSubscriptionsForEvent<T>() where T : IntegrationEvent;

    /// <summary>
    /// 判断是否存在该事件名称的订阅。
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <returns>存在返回true，否则返回false</returns>
    bool HasSubscriptionsForEvent(string eventName);

    /// <summary>
    /// 根据事件名称获取事件对应的具体类型。
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <returns>事件对应的Type</returns>
    Type GetEventTypeByName(string eventName);

    /// <summary>
    /// 清除所有已注册的订阅。
    /// </summary>
    void Clear();

    /// <summary>
    /// 根据事件类型获取所有订阅信息。
    /// </summary>
    /// <typeparam name="T">事件类型</typeparam>
    /// <returns>订阅信息集合</returns>
    IEnumerable<SubscriptionInfo> GetHandlersForEvent<T>() where T : IntegrationEvent;

    /// <summary>
    /// 根据事件名称获取所有订阅信息。
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <returns>订阅信息集合</returns>
    IEnumerable<SubscriptionInfo> GetHandlersForEvent(string eventName);

    /// <summary>
    /// 获取指定事件类型对应的事件名称，用于在订阅字典中查找。
    /// </summary>
    /// <typeparam name="T">事件类型</typeparam>
    /// <returns>事件名称</returns>
    string GetEventKey<T>();
}
