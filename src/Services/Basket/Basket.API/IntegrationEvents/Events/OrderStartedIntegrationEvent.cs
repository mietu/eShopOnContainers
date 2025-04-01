namespace Basket.API.IntegrationEvents.Events;

// OrderStartedIntegrationEvent代表订单启动的集成事件
// 此事件用于在跨服务通信中通知其他系统用户已开始订单流程
public record OrderStartedIntegrationEvent : IntegrationEvent
{
    // 用户标识符，用于追踪触发订单的具体用户
    public string UserId { get; init; }

    // 构造函数：接收用户标识符并初始化事件
    // 同时调用基类默认构造函数生成事件唯一标识和创建时间
    public OrderStartedIntegrationEvent(string userId)
        : base() // 调用IntegrationEvent基类的默认构造函数以初始化Id和CreationDate
        => UserId = userId;
}
