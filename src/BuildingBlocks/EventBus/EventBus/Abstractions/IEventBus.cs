namespace Microsoft.eShopOnContainers.BuildingBlocks.EventBus.Abstractions;

/// <summary>
/// 定义事件总线接口，用于发布和订阅集成事件。
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// 发布集成事件。
    /// </summary>
    /// <param name="event">
    /// 要发布的集成事件对象，包含事件标识和创建日期等信息。
    /// </param>
    void Publish(IntegrationEvent @event);

    /// <summary>
    /// 订阅指定的集成事件，并指定处理该事件的事件处理器类型。
    /// </summary>
    /// <typeparam name="T">
    /// 集成事件的具体类型，继承自 IntegrationEvent。
    /// </typeparam>
    /// <typeparam name="TH">
    /// 用于处理该集成事件的事件处理器类型，必须实现 IIntegrationEventHandler&lt;T&gt; 接口。
    /// </typeparam>
    void Subscribe<T, TH>()
        where T : IntegrationEvent
        where TH : IIntegrationEventHandler<T>;

    /// <summary>
    /// 动态订阅指定名称的集成事件，使用动态事件处理器。
    /// 注：动态处理器适用于事件类型在编译期未知的情况。
    /// </summary>
    /// <typeparam name="TH">
    /// 用于处理事件的动态事件处理器类型，必须实现 IDynamicIntegrationEventHandler 接口。
    /// </typeparam>
    /// <param name="eventName">
    /// 事件名称，可用作事件路由或标识。
    /// </param>
    void SubscribeDynamic<TH>(string eventName)
        where TH : IDynamicIntegrationEventHandler;

    /// <summary>
    /// 对动态事件处理器取消订阅指定名称的事件。
    /// </summary>
    /// <typeparam name="TH">
    /// 动态事件处理器类型，必须实现 IDynamicIntegrationEventHandler 接口。
    /// </typeparam>
    /// <param name="eventName">
    /// 要取消订阅的事件名称。
    /// </param>
    void UnsubscribeDynamic<TH>(string eventName)
        where TH : IDynamicIntegrationEventHandler;

    /// <summary>
    /// 取消订阅指定的事件以及对应的事件处理器。
    /// </summary>
    /// <typeparam name="T">
    /// 集成事件的具体类型，继承自 IntegrationEvent。
    /// </typeparam>
    /// <typeparam name="TH">
    /// 用于处理该集成事件的事件处理器类型，必须实现 IIntegrationEventHandler&lt;T&gt; 接口。
    /// </typeparam>
    void Unsubscribe<T, TH>()
        where TH : IIntegrationEventHandler<T>
        where T : IntegrationEvent;
}
