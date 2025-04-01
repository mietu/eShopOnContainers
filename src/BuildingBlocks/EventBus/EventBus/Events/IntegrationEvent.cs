namespace Microsoft.eShopOnContainers.BuildingBlocks.EventBus.Events;

/// <summary>
/// 集成事件基类，用于在不同服务间传递事件信息
/// </summary>
public record IntegrationEvent
{
    /// <summary>
    /// 默认构造函数：生成一个新的唯一标识并设置创建时间
    /// </summary>
    public IntegrationEvent()
    {
        // 创建一个全局唯一标识符，用于识别事件
        Id = Guid.NewGuid();
        // 记录事件创建时的UTC时间
        CreationDate = DateTime.UtcNow;
    }

    /// <summary>
    /// 使用 JsonConstructor 构造函数，用于 JSON 反序列化时还原事件属性
    /// </summary>
    /// <param name="id">事件的唯一标识</param>
    /// <param name="createDate">事件的创建时间</param>
    [JsonConstructor]
    public IntegrationEvent(Guid id, DateTime createDate)
    {
        Id = id;
        CreationDate = createDate;
    }

    /// <summary>
    /// 事件的唯一标识，通过 JsonInclude 标记确保在序列化和反序列化过程中包含此属性
    /// </summary>
    [JsonInclude]
    public Guid Id { get; private init; }

    /// <summary>
    /// 事件创建的UTC时间，通过 JsonInclude 标记确保在序列化和反序列化过程中包含此属性
    /// </summary>
    [JsonInclude]
    public DateTime CreationDate { get; private init; }
}
