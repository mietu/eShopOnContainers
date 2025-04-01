namespace Microsoft.eShopOnContainers.BuildingBlocks.EventBus.Abstractions;

/// <summary>
/// 动态集成事件处理器接口
/// 定义一个处理动态事件数据的方法，可以在运行时解析事件数据结构
/// </summary>
public interface IDynamicIntegrationEventHandler
{
    /// <summary>
    /// 处理传入的动态事件数据。
    /// 使用 dynamic 类型允许在运行时处理不确定类型的事件数据。
    /// </summary>
    /// <param name="eventData">动态事件数据，结构在运行时确定</param>
    /// <returns>异步任务，处理完成后返回</returns>
    Task Handle(dynamic eventData);
}
