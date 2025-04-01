namespace Microsoft.eShopOnContainers.BuildingBlocks.EventBus
{
    // InMemoryEventBusSubscriptionsManager 用于基于内存管理事件订阅，支持强类型和动态类型事件订阅
    public partial class InMemoryEventBusSubscriptionsManager : IEventBusSubscriptionsManager
    {
        // 定义事件名称和对应事件订阅的字典映射（1:N）
        private readonly Dictionary<string, List<SubscriptionInfo>> _handlers;
        // 用于保存所有事件对应的处理类型
        private readonly List<Type> _eventTypes;
        // 定义移除事件后触发的事件，提供事件名称
        public event EventHandler<string> OnEventRemoved;

        // 构造函数：初始化字典和列表
        public InMemoryEventBusSubscriptionsManager()
        {
            _handlers = new Dictionary<string, List<SubscriptionInfo>>();
            _eventTypes = new List<Type>();
        }

        // 判断是否存在任何订阅，若 _handlers 字典中没有数据则返回 true
        public bool IsEmpty => _handlers is { Count: 0 };

        // 清空所有订阅
        public void Clear() => _handlers.Clear();

        // 添加动态类型的事件订阅，需要指定事件名称
        // 注：动态订阅的处理器实现接口为 IDynamicIntegrationEventHandler
        public void AddDynamicSubscription<TH>(string eventName)
            where TH : IDynamicIntegrationEventHandler
        {
            DoAddSubscription(typeof(TH), eventName, isDynamic: true);
        }

        // 添加强类型事件订阅，事件名称通过 IntegrationEvent 类型的名称自动生成
        // 注：强类型订阅的处理器实现接口为 IIntegrationEventHandler<T>
        public void AddSubscription<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = GetEventKey<T>();

            DoAddSubscription(typeof(TH), eventName, isDynamic: false);

            // 如果事件类型未注册，添加到 _eventTypes 列表中
            if (!_eventTypes.Contains(typeof(T)))
            {
                _eventTypes.Add(typeof(T));
            }
        }

        // 核心方法，根据传入的 handlerType、事件名称及是否动态添加订阅
        private void DoAddSubscription(Type handlerType, string eventName, bool isDynamic)
        {
            if (!HasSubscriptionsForEvent(eventName))
            {
                _handlers.Add(eventName, new List<SubscriptionInfo>());
            }

            // 如果已经注册，则抛出异常
            if (_handlers[eventName].Any(s => s.HandlerType == handlerType))
            {
                throw new ArgumentException(
                    $"Handler Type {handlerType.Name} already registered for '{eventName}'", nameof(handlerType));
            }

            // 根据是否动态添加对应类型的 SubscriptionInfo
            if (isDynamic)
            {
                _handlers[eventName].Add(SubscriptionInfo.Dynamic(handlerType));
            }
            else
            {
                _handlers[eventName].Add(SubscriptionInfo.Typed(handlerType));
            }
        }

        // 移除动态订阅
        public void RemoveDynamicSubscription<TH>(string eventName)
            where TH : IDynamicIntegrationEventHandler
        {
            var handlerToRemove = FindDynamicSubscriptionToRemove<TH>(eventName);
            DoRemoveHandler(eventName, handlerToRemove);
        }

        // 移除强类型订阅
        public void RemoveSubscription<T, TH>()
            where TH : IIntegrationEventHandler<T>
            where T : IntegrationEvent
        {
            var handlerToRemove = FindSubscriptionToRemove<T, TH>();
            var eventName = GetEventKey<T>();
            DoRemoveHandler(eventName, handlerToRemove);
        }

        // 核心移除方法，通过事件名称移除指定的 SubscriptionInfo，并在无订阅时触发事件移除通知
        private void DoRemoveHandler(string eventName, SubscriptionInfo subsToRemove)
        {
            if (subsToRemove != null)
            {
                _handlers[eventName].Remove(subsToRemove);
                if (!_handlers[eventName].Any())
                {
                    _handlers.Remove(eventName);
                    var eventType = _eventTypes.SingleOrDefault(e => e.Name == eventName);
                    if (eventType != null)
                    {
                        _eventTypes.Remove(eventType);
                    }
                    RaiseOnEventRemoved(eventName);
                }
            }
        }

        // 获取指定 IntegrationEvent 类型的所有订阅处理器
        public IEnumerable<SubscriptionInfo> GetHandlersForEvent<T>() where T : IntegrationEvent
        {
            var key = GetEventKey<T>();
            return GetHandlersForEvent(key);
        }

        // 根据事件名称获取所有订阅处理器列表
        public IEnumerable<SubscriptionInfo> GetHandlersForEvent(string eventName) => _handlers[eventName];

        // 触发事件移除后的回调，通知外部订阅者
        private void RaiseOnEventRemoved(string eventName)
        {
            var handler = OnEventRemoved;
            handler?.Invoke(this, eventName);
        }

        // 根据传入的泛型处理器类型查找对应的动态订阅
        private SubscriptionInfo FindDynamicSubscriptionToRemove<TH>(string eventName)
            where TH : IDynamicIntegrationEventHandler
        {
            return DoFindSubscriptionToRemove(eventName, typeof(TH));
        }

        // 根据传入的泛型参数查找对应的强类型订阅
        private SubscriptionInfo FindSubscriptionToRemove<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = GetEventKey<T>();
            return DoFindSubscriptionToRemove(eventName, typeof(TH));
        }

        // 在指定事件名称的订阅列表中查找某一处理类型的订阅信息
        private SubscriptionInfo DoFindSubscriptionToRemove(string eventName, Type handlerType)
        {
            if (!HasSubscriptionsForEvent(eventName))
            {
                return null;
            }
            return _handlers[eventName].SingleOrDefault(s => s.HandlerType == handlerType);
        }

        // 检查指定 IntegrationEvent 类型是否已存在订阅
        public bool HasSubscriptionsForEvent<T>() where T : IntegrationEvent
        {
            var key = GetEventKey<T>();
            return HasSubscriptionsForEvent(key);
        }

        // 检查指定事件名称是否已有订阅记录
        public bool HasSubscriptionsForEvent(string eventName) => _handlers.ContainsKey(eventName);

        // 根据事件名称返回注册的事件类型对象
        public Type GetEventTypeByName(string eventName) => _eventTypes.SingleOrDefault(t => t.Name == eventName);

        // 用于获取 IntegrationEvent 类型对应的事件名称（这里直接使用类型名称）
        public string GetEventKey<T>()
        {
            return typeof(T).Name;
        }
    }
}
