namespace Microsoft.eShopOnContainers.BuildingBlocks.IntegrationEventLogEF;

/// <summary>
/// 集成事件日志条目，用于记录集成事件相关信息
/// </summary>
public class IntegrationEventLogEntry
{
    // 私有默认构造函数，防止外部直接实例化
    private IntegrationEventLogEntry() { }

    /// <summary>
    /// 构造函数，接收集成事件和事务ID，初始化日志条目相关属性
    /// </summary>
    /// <param name="event">集成事件对象</param>
    /// <param name="transactionId">关联的事务ID</param>
    public IntegrationEventLogEntry(IntegrationEvent @event, Guid transactionId)
    {
        // 事件唯一标识取自事件对象
        EventId = @event.Id;
        // 记录事件创建时间
        CreationTime = @event.CreationDate;
        // 记录事件类型全名
        EventTypeName = @event.GetType().FullName;
        // 将事件内容序列化为JSON字符串，格式化输出（缩进）便于阅读
        Content = JsonSerializer.Serialize(@event, @event.GetType(), new JsonSerializerOptions
        {
            WriteIndented = true
        });
        // 设置初始状态为未发布
        State = EventStateEnum.NotPublished;
        // 初始化发送次数为0
        TimesSent = 0;
        // 将事务ID转换为字符串保存
        TransactionId = transactionId.ToString();
    }

    /// <summary>
    /// 事件的唯一标识
    /// </summary>
    public Guid EventId { get; private set; }

    /// <summary>
    /// 存储事件类型的全名
    /// </summary>
    public string EventTypeName { get; private set; }

    /// <summary>
    /// 获取事件类型的短名称，取最后一个部分
    /// 使用 NotMapped 属性防止被映射到数据库
    /// </summary>
    [NotMapped]
    public string EventTypeShortName => EventTypeName.Split('.')?.Last();

    /// <summary>
    /// 序列化前的集成事件对象
    /// 使用 NotMapped 属性防止被映射到数据库
    /// </summary>
    [NotMapped]
    public IntegrationEvent IntegrationEvent { get; private set; }

    /// <summary>
    /// 事件当前状态（未发布、进行中、已发布、发布失败）
    /// </summary>
    public EventStateEnum State { get; set; }

    /// <summary>
    /// 发送次数计数器
    /// </summary>
    public int TimesSent { get; set; }

    /// <summary>
    /// 事件日志创建时间
    /// </summary>
    public DateTime CreationTime { get; private set; }

    /// <summary>
    /// 事件内容的序列化表示（JSON格式）
    /// </summary>
    public string Content { get; private set; }

    /// <summary>
    /// 与该事件关联的事务ID（字符串格式）
    /// </summary>
    public string TransactionId { get; private set; }

    /// <summary>
    /// 反序列化JSON内容，将Content转换回原始的集成事件对象
    /// </summary>
    /// <param name="type">目标反序列化的类型</param>
    /// <returns>返回当前集成事件日志条目，IntegrationEvent属性已被赋值</returns>
    public IntegrationEventLogEntry DeserializeJsonContent(Type type)
    {
        IntegrationEvent = JsonSerializer.Deserialize(Content, type, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true }) as IntegrationEvent;
        return this;
    }
}
