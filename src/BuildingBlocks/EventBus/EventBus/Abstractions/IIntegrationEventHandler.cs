namespace Microsoft.eShopOnContainers.BuildingBlocks.EventBus.Abstractions
{
    /// <summary>
    /// 定义用于处理集成事件的泛型接口。
    /// 此接口继承自 IIntegrationEventHandler，并约束 TIntegrationEvent 必须是 IntegrationEvent 的子类。
    /// </summary>
    /// <typeparam name="TIntegrationEvent">集成事件的具体类型，必须继承自 IntegrationEvent。</typeparam>
    public interface IIntegrationEventHandler<in TIntegrationEvent> : IIntegrationEventHandler
        where TIntegrationEvent : IntegrationEvent
    {
        // 当接收到特定类型的集成事件时调用该方法进行处理
        Task Handle(TIntegrationEvent @event);
    }

    /// <summary>
    /// 定义所有集成事件处理程序的标记接口。
    /// 它不包含成员，但用于统一标识所有集成事件处理程序。
    /// </summary>
    public interface IIntegrationEventHandler
    {
    }
}
