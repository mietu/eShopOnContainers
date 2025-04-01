namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.IntegrationEvents.Events;

// 记录类型 OrderStartedIntegrationEvent 继承 IntegrationEvent
// 此事件表示订单已启动，可以被其他微服务或外部系统使用
public record OrderStartedIntegrationEvent : IntegrationEvent
{
    // 表示用户的标识符，用于跟踪是谁启动了订单
    public string UserId { get; init; }

    // 构造函数，接收一个用户标识符字符串参数
    // 参数 userId 被赋值给只读属性 UserId
    public OrderStartedIntegrationEvent(string userId)
        => UserId = userId;
}
