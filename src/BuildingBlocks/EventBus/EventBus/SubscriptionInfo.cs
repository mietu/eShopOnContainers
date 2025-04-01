namespace Microsoft.eShopOnContainers.BuildingBlocks.EventBus;

// 部分类，表示事件总线订阅管理器的一部分实现
public partial class InMemoryEventBusSubscriptionsManager : IEventBusSubscriptionsManager
{
    // 订阅信息类，存储订阅处理程序的类型及其是否为动态订阅
    public class SubscriptionInfo
    {
        // 属性：标识该订阅是否为动态订阅
        public bool IsDynamic { get; }
        // 属性：存储订阅处理程序的具体类型
        public Type HandlerType { get; }

        // 私有构造函数，用于初始化 SubscriptionInfo 实例
        // 参数 isDynamic 表示是否为动态订阅，handlerType 表示处理程序的类型
        private SubscriptionInfo(bool isDynamic, Type handlerType)
        {
            IsDynamic = isDynamic;
            HandlerType = handlerType;
        }

        // 静态方法，创建一个动态订阅信息实例
        // 参数 handlerType 为处理程序的类型
        public static SubscriptionInfo Dynamic(Type handlerType) =>
            new SubscriptionInfo(true, handlerType);

        // 静态方法，创建一个静态（类型化）订阅信息实例
        // 参数 handlerType 为处理程序的类型
        public static SubscriptionInfo Typed(Type handlerType) =>
            new SubscriptionInfo(false, handlerType);
    }
}
