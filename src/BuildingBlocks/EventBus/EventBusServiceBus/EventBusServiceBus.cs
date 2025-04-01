namespace Microsoft.eShopOnContainers.BuildingBlocks.EventBusServiceBus;

// EventBusServiceBus 类实现了 IEventBus 接口和 IAsyncDisposable 接口，用于处理 Service Bus 的消息发布和订阅逻辑
public class EventBusServiceBus : IEventBus, IAsyncDisposable
{
    // 定义 Service Bus 持久连接、日志、订阅管理器、Autofac 依赖注入容器等实例
    private readonly IServiceBusPersisterConnection _serviceBusPersisterConnection;
    private readonly ILogger<EventBusServiceBus> _logger;
    private readonly IEventBusSubscriptionsManager _subsManager;
    private readonly ILifetimeScope _autofac;
    private readonly string _topicName = "eshop_event_bus"; // 主题名称
    private readonly string _subscriptionName;
    private readonly ServiceBusSender _sender;
    private readonly ServiceBusProcessor _processor;
    private readonly string AUTOFAC_SCOPE_NAME = "eshop_event_bus";
    private const string INTEGRATION_EVENT_SUFFIX = "IntegrationEvent";

    // 构造函数，初始化各依赖对象和 Service Bus 相关实体
    public EventBusServiceBus(IServiceBusPersisterConnection serviceBusPersisterConnection,
        ILogger<EventBusServiceBus> logger, IEventBusSubscriptionsManager subsManager, ILifetimeScope autofac, string subscriptionClientName)
    {
        // 初始化字段
        _serviceBusPersisterConnection = serviceBusPersisterConnection;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _subsManager = subsManager ?? new InMemoryEventBusSubscriptionsManager();
        _autofac = autofac;
        _subscriptionName = subscriptionClientName;

        // 创建发送器，用于向指定主题发送消息
        _sender = _serviceBusPersisterConnection.TopicClient.CreateSender(_topicName);

        // 配置消息处理器选项，设置最大并发数和自动完成消息为 false
        ServiceBusProcessorOptions options = new ServiceBusProcessorOptions { MaxConcurrentCalls = 10, AutoCompleteMessages = false };
        // 创建消息处理器，用于处理接收到的消息
        _processor = _serviceBusPersisterConnection.TopicClient.CreateProcessor(_topicName, _subscriptionName, options);

        // 删除默认规则，防止无关消息进入处理逻辑
        RemoveDefaultRule();
        // 注册消息处理器回调，并启动异步处理过程
        RegisterSubscriptionClientMessageHandlerAsync().GetAwaiter().GetResult();
    }

    // 发布集成事件
    public void Publish(IntegrationEvent @event)
    {
        // 根据事件对象类型的名称创建事件名称（去掉后缀）
        var eventName = @event.GetType().Name.Replace(INTEGRATION_EVENT_SUFFIX, "");
        // 将事件对象序列化为 JSON 字符串
        var jsonMessage = JsonSerializer.Serialize(@event, @event.GetType());
        // 将 JSON 字符串转换为字节数组
        var body = Encoding.UTF8.GetBytes(jsonMessage);

        // 构造 Service Bus 消息对象
        var message = new ServiceBusMessage
        {
            MessageId = Guid.NewGuid().ToString(),
            Body = new BinaryData(body),
            Subject = eventName, // 使用事件名称作为消息主题
        };

        // 异步发送消息，但此处同步等待结果
        _sender.SendMessageAsync(message)
            .GetAwaiter()
            .GetResult();
    }

    // 订阅动态事件
    public void SubscribeDynamic<TH>(string eventName)
        where TH : IDynamicIntegrationEventHandler
    {
        _logger.LogInformation("Subscribing to dynamic event {EventName} with {EventHandler}", eventName, typeof(TH).Name);
        // 添加动态订阅到订阅管理器中
        _subsManager.AddDynamicSubscription<TH>(eventName);
    }

    // 订阅静态类型的事件
    public void Subscribe<T, TH>()
        where T : IntegrationEvent
        where TH : IIntegrationEventHandler<T>
    {
        // 根据类型获取事件名称
        var eventName = typeof(T).Name.Replace(INTEGRATION_EVENT_SUFFIX, "");

        // 检查订阅管理器中是否已存在该事件的订阅
        var containsKey = _subsManager.HasSubscriptionsForEvent<T>();
        if (!containsKey)
        {
            try
            {
                // 如果不存在，则通过 AdministrationClient 创建 Service Bus 规则
                _serviceBusPersisterConnection.AdministrationClient.CreateRuleAsync(_topicName, _subscriptionName, new CreateRuleOptions
                {
                    Filter = new CorrelationRuleFilter() { Subject = eventName },
                    Name = eventName
                }).GetAwaiter().GetResult();
            }
            catch (ServiceBusException)
            {
                // 如果规则已存在，记录警告信息
                _logger.LogWarning("The messaging entity {eventName} already exists.", eventName);
            }
        }

        _logger.LogInformation("Subscribing to event {EventName} with {EventHandler}", eventName, typeof(TH).Name);

        // 添加订阅信息到订阅管理器中
        _subsManager.AddSubscription<T, TH>();
    }

    // 取消订阅静态类型的事件
    public void Unsubscribe<T, TH>()
        where T : IntegrationEvent
        where TH : IIntegrationEventHandler<T>
    {
        // 根据类型获取事件名称
        var eventName = typeof(T).Name.Replace(INTEGRATION_EVENT_SUFFIX, "");

        try
        {
            // 通过 AdministrationClient 删除 Service Bus 规则
            _serviceBusPersisterConnection
                .AdministrationClient
                .DeleteRuleAsync(_topicName, _subscriptionName, eventName)
                .GetAwaiter()
                .GetResult();
        }
        catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.MessagingEntityNotFound)
        {
            // 如果找不到规则，记录警告信息
            _logger.LogWarning("The messaging entity {eventName} Could not be found.", eventName);
        }

        _logger.LogInformation("Unsubscribing from event {EventName}", eventName);

        // 从订阅管理器中移除订阅信息
        _subsManager.RemoveSubscription<T, TH>();
    }

    // 取消订阅动态事件
    public void UnsubscribeDynamic<TH>(string eventName)
        where TH : IDynamicIntegrationEventHandler
    {
        _logger.LogInformation("Unsubscribing from dynamic event {EventName}", eventName);
        // 从订阅管理器中移除动态订阅
        _subsManager.RemoveDynamicSubscription<TH>(eventName);
    }

    // 注册处理 Service Bus 消息的回调函数，并启动消息处理
    private async Task RegisterSubscriptionClientMessageHandlerAsync()
    {
        // 注册消息处理委托
        _processor.ProcessMessageAsync +=
            async (args) =>
            {
                // 回复 Service Bus 的 Subject 前缀添加后缀还原完整事件名称
                var eventName = $"{args.Message.Subject}{INTEGRATION_EVENT_SUFFIX}";
                // 将消息体转换为字符串获取消息数据
                string messageData = args.Message.Body.ToString();

                // 处理事件，如果处理成功则完成消息，避免重复接收
                if (await ProcessEvent(eventName, messageData))
                {
                    await args.CompleteMessageAsync(args.Message);
                }
            };

        // 注册错误处理回调
        _processor.ProcessErrorAsync += ErrorHandler;
        // 开启消息处理
        await _processor.StartProcessingAsync();
    }

    // 错误处理方法，捕获消息处理过程中发生的异常并记录日志
    private Task ErrorHandler(ProcessErrorEventArgs args)
    {
        var ex = args.Exception;
        var context = args.ErrorSource;

        _logger.LogError(ex, "ERROR handling message: {ExceptionMessage} - Context: {@ExceptionContext}", ex.Message, context);

        return Task.CompletedTask;
    }

    // 根据事件名称和消息数据处理集成事件，并调用相应的事件处理器
    private async Task<bool> ProcessEvent(string eventName, string message)
    {
        var processed = false;
        // 检查当前是否存在该事件的订阅
        if (_subsManager.HasSubscriptionsForEvent(eventName))
        {
            // 使用 Autofac 开启新的生命周期范围
            var scope = _autofac.BeginLifetimeScope(AUTOFAC_SCOPE_NAME);
            // 获取所有对应事件的处理器订阅
            var subscriptions = _subsManager.GetHandlersForEvent(eventName);
            foreach (var subscription in subscriptions)
            {
                if (subscription.IsDynamic)
                {
                    // 处理动态订阅：通过依赖注入解析处理器，并使用 JsonDocument 处理动态事件数据
                    if (scope.ResolveOptional(subscription.HandlerType) is not IDynamicIntegrationEventHandler handler) continue;
                    using dynamic eventData = JsonDocument.Parse(message);
                    await handler.Handle(eventData);
                }
                else
                {
                    // 处理静态类型订阅：解析具体事件类型并调用对应接口处理方法
                    var handler = scope.ResolveOptional(subscription.HandlerType);
                    if (handler == null) continue;
                    var eventType = _subsManager.GetEventTypeByName(eventName);
                    var integrationEvent = JsonSerializer.Deserialize(message, eventType);
                    var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
                    await (Task)concreteType.GetMethod("Handle").Invoke(handler, new[] { integrationEvent });
                }
            }
        }
        // 标识消息处理已完成，防止重复处理
        processed = true;
        return processed;
    }

    // 删除默认规则，防止默认消息路由规则影响订阅操作
    private void RemoveDefaultRule()
    {
        try
        {
            _serviceBusPersisterConnection
                .AdministrationClient
                .DeleteRuleAsync(_topicName, _subscriptionName, RuleProperties.DefaultRuleName)
                .GetAwaiter()
                .GetResult();
        }
        catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.MessagingEntityNotFound)
        {
            _logger.LogWarning("The messaging entity {DefaultRuleName} Could not be found.", RuleProperties.DefaultRuleName);
        }
    }

    // 实现 IAsyncDisposable 接口，释放订阅管理器并关闭消息处理器
    public async ValueTask DisposeAsync()
    {
        _subsManager.Clear();
        await _processor.CloseAsync();
    }
}
