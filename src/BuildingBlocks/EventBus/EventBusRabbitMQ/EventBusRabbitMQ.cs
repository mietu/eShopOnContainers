namespace Microsoft.eShopOnContainers.BuildingBlocks.EventBusRabbitMQ;

/// <summary>
/// EventBusRabbitMQ是基于RabbitMQ的事件总线实现
/// 负责发布消息、订阅消息和消费消息，并管理事件路由与处理
/// </summary>
public class EventBusRabbitMQ : IEventBus, IDisposable
{
    // 定义交换机名称以及Autofac作用域名称常量
    const string BROKER_NAME = "eshop_event_bus";
    const string AUTOFAC_SCOPE_NAME = "eshop_event_bus";

    private readonly IRabbitMQPersistentConnection _persistentConnection;
    private readonly ILogger<EventBusRabbitMQ> _logger;
    private readonly IEventBusSubscriptionsManager _subsManager;
    private readonly ILifetimeScope _autofac;
    private readonly int _retryCount;

    // 处理消费的信道及队列名称
    private IModel _consumerChannel;
    private string _queueName;

    /// <summary>
    /// 构造函数，进行依赖注入和初始化
    /// </summary>
    public EventBusRabbitMQ(IRabbitMQPersistentConnection persistentConnection, ILogger<EventBusRabbitMQ> logger,
        ILifetimeScope autofac, IEventBusSubscriptionsManager subsManager, string queueName = null, int retryCount = 5)
    {
        // 注入RabbitMQ连接，日志和订阅管理器
        _persistentConnection = persistentConnection ?? throw new ArgumentNullException(nameof(persistentConnection));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        // 如果未提供订阅管理器，则使用空对象模式
        _subsManager = subsManager ?? new InMemoryEventBusSubscriptionsManager();
        _queueName = queueName;
        // 创建消费者信道，用于接收消息
        _consumerChannel = CreateConsumerChannel();
        _autofac = autofac;
        _retryCount = retryCount;

        // 注册当事件被移除时触发的回调，取消绑定队列
        _subsManager.OnEventRemoved += SubsManager_OnEventRemoved;
    }

    /// <summary>
    /// 当订阅管理器移除事件时调用，取消该事件在队列中的绑定
    /// </summary>
    private void SubsManager_OnEventRemoved(object sender, string eventName)
    {
        // 确保RabbitMQ连接处于连接状态
        if (!_persistentConnection.IsConnected)
        {
            _persistentConnection.TryConnect();
        }

        using var channel = _persistentConnection.CreateModel();
        // 取消指定事件和队列的绑定
        channel.QueueUnbind(queue: _queueName,
            exchange: BROKER_NAME,
            routingKey: eventName);

        // 如果无任何订阅，则关闭消费者信道
        if (_subsManager.IsEmpty)
        {
            _queueName = string.Empty;
            _consumerChannel.Close();
        }
    }

    /// <summary>
    /// 发布集成事件到RabbitMQ
    /// </summary>
    public void Publish(IntegrationEvent @event)
    {
        // 如果未连接则尝试重连
        if (!_persistentConnection.IsConnected)
        {
            _persistentConnection.TryConnect();
        }

        // 定义Polly重试策略，通过指数退避进行重试
        var policy = RetryPolicy.Handle<BrokerUnreachableException>()
            .Or<SocketException>()
            .WaitAndRetry(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
            {
                _logger.LogWarning(ex, "Could not publish event: {EventId} after {Timeout}s ({ExceptionMessage})", @event.Id, $"{time.TotalSeconds:n1}", ex.Message);
            });

        // 获取事件名称
        var eventName = @event.GetType().Name;

        _logger.LogTrace("Creating RabbitMQ channel to publish event: {EventId} ({EventName})", @event.Id, eventName);

        using var channel = _persistentConnection.CreateModel();
        _logger.LogTrace("Declaring RabbitMQ exchange to publish event: {EventId}", @event.Id);

        // 声明交换机并采用direct类型进行消息路由
        channel.ExchangeDeclare(exchange: BROKER_NAME, type: "direct");

        // 将事件序列化为Json格式字节数组（消息体）
        var body = JsonSerializer.SerializeToUtf8Bytes(@event, @event.GetType(), new JsonSerializerOptions
        {
            WriteIndented = true
        });

        // 执行重试策略下的消息发布逻辑
        policy.Execute(() =>
        {
            var properties = channel.CreateBasicProperties();
            // 标记消息为持久化（DeliveryMode = 2）
            properties.DeliveryMode = 2;

            _logger.LogTrace("Publishing event to RabbitMQ: {EventId}", @event.Id);

            // 发布消息，设置mandatory标记为true以确保未找到队列时返回消息
            channel.BasicPublish(
                exchange: BROKER_NAME,
                routingKey: eventName,
                mandatory: true,
                basicProperties: properties,
                body: body);
        });
    }

    /// <summary>
    /// 动态订阅指定名称的事件，使用提供的动态处理器类型
    /// </summary>
    public void SubscribeDynamic<TH>(string eventName)
        where TH : IDynamicIntegrationEventHandler
    {
        _logger.LogInformation("Subscribing to dynamic event {EventName} with {EventHandler}", eventName, typeof(TH).GetGenericTypeName());

        // 内部订阅绑定队列与路由键
        DoInternalSubscription(eventName);
        // 注册动态订阅
        _subsManager.AddDynamicSubscription<TH>(eventName);
        // 启动消费监听
        StartBasicConsume();
    }

    /// <summary>
    /// 订阅静态类型事件及其对应的处理器类型
    /// </summary>
    public void Subscribe<T, TH>()
        where T : IntegrationEvent
        where TH : IIntegrationEventHandler<T>
    {
        // 使用订阅管理器生成事件路由键
        var eventName = _subsManager.GetEventKey<T>();
        DoInternalSubscription(eventName);

        _logger.LogInformation("Subscribing to event {EventName} with {EventHandler}", eventName, typeof(TH).GetGenericTypeName());

        // 添加静态订阅到管理器中
        _subsManager.AddSubscription<T, TH>();
        // 启动消费者监听
        StartBasicConsume();
    }

    /// <summary>
    /// 内部处理订阅逻辑：当还没有该事件的订阅时，绑定队列与事件对应的路由键
    /// </summary>
    private void DoInternalSubscription(string eventName)
    {
        var containsKey = _subsManager.HasSubscriptionsForEvent(eventName);
        if (!containsKey)
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            // 将队列和交换机按照事件名称进行绑定
            _consumerChannel.QueueBind(queue: _queueName,
                                exchange: BROKER_NAME,
                                routingKey: eventName);
        }
    }

    /// <summary>
    /// 取消静态订阅事件及其处理器
    /// </summary>
    public void Unsubscribe<T, TH>()
        where T : IntegrationEvent
        where TH : IIntegrationEventHandler<T>
    {
        var eventName = _subsManager.GetEventKey<T>();

        _logger.LogInformation("Unsubscribing from event {EventName}", eventName);

        _subsManager.RemoveSubscription<T, TH>();
    }

    /// <summary>
    /// 取消动态订阅
    /// </summary>
    public void UnsubscribeDynamic<TH>(string eventName)
        where TH : IDynamicIntegrationEventHandler
    {
        _subsManager.RemoveDynamicSubscription<TH>(eventName);
    }

    /// <summary>
    /// 清理资源，关闭消费者信道并清空订阅管理器
    /// </summary>
    public void Dispose()
    {
        if (_consumerChannel != null)
        {
            _consumerChannel.Dispose();
        }

        _subsManager.Clear();
    }

    /// <summary>
    /// 启动基础的消费监听，通过注册Received事件处理传入的消息
    /// </summary>
    private void StartBasicConsume()
    {
        _logger.LogTrace("Starting RabbitMQ basic consume");

        if (_consumerChannel != null)
        {
            var consumer = new AsyncEventingBasicConsumer(_consumerChannel);

            // 将消费者的Received事件绑定到本类的处理方法上
            consumer.Received += Consumer_Received;

            _consumerChannel.BasicConsume(
                queue: _queueName,
                autoAck: false,
                consumer: consumer);
        }
        else
        {
            _logger.LogError("StartBasicConsume can't call on _consumerChannel == null");
        }
    }

    /// <summary>
    /// 消费者收到消息后的事件处理方法
    /// 解析消息，执行相关的事件处理逻辑，并处理异常
    /// </summary>
    private async Task Consumer_Received(object sender, BasicDeliverEventArgs eventArgs)
    {
        var eventName = eventArgs.RoutingKey;
        // 将消息体解码为UTF8字符串
        var message = Encoding.UTF8.GetString(eventArgs.Body.Span);

        try
        {
            // 当消息包含指定关键字时，模拟异常
            if (message.ToLowerInvariant().Contains("throw-fake-exception"))
            {
                throw new InvalidOperationException($"Fake exception requested: \"{message}\"");
            }

            // 处理接收到的事件消息
            await ProcessEvent(eventName, message);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "----- ERROR Processing message \"{Message}\"", message);
        }

        // 无论处理是否出错，都确认消息，以防止消息再次投递
        // 实际应用中可使用死信队列（DLX）进行失败消息处理，详情参考RabbitMQ文档
        _consumerChannel.BasicAck(eventArgs.DeliveryTag, multiple: false);
    }

    /// <summary>
    /// 创建消费者信道，声明交换机与队列，并绑定异常处理事件
    /// </summary>
    private IModel CreateConsumerChannel()
    {
        if (!_persistentConnection.IsConnected)
        {
            _persistentConnection.TryConnect();
        }

        _logger.LogTrace("Creating RabbitMQ consumer channel");

        // 创建信道，用于消息消费
        var channel = _persistentConnection.CreateModel();

        // 声明交换机，类型为direct以实现全匹配路由
        channel.ExchangeDeclare(exchange: BROKER_NAME, type: "direct");

        // 声明队列，持久化、不排他、不上自动删除
        channel.QueueDeclare(queue: _queueName,
                                durable: true,
                                exclusive: false,
                                autoDelete: false,
                                arguments: null);

        // 当信道出现异常时，重建信道并恢复消费监听
        channel.CallbackException += (sender, ea) =>
        {
            _logger.LogWarning(ea.Exception, "Recreating RabbitMQ consumer channel");

            _consumerChannel.Dispose();
            _consumerChannel = CreateConsumerChannel();
            StartBasicConsume();
        };

        return channel;
    }

    /// <summary>
    /// 根据事件名称和消息内容处理对应的事件
    /// 包括通过DI解析处理器、反序列化事件、调用处理方法等步骤
    /// </summary>
    private async Task ProcessEvent(string eventName, string message)
    {
        _logger.LogTrace("Processing RabbitMQ event: {EventName}", eventName);

        // 判断订阅管理器中是否存在该事件相关订阅
        if (_subsManager.HasSubscriptionsForEvent(eventName))
        {
            // 利用Autofac创建新的依赖解析作用域
            await using var scope = _autofac.BeginLifetimeScope(AUTOFAC_SCOPE_NAME);
            var subscriptions = _subsManager.GetHandlersForEvent(eventName);
            foreach (var subscription in subscriptions)
            {
                // 动态订阅处理
                if (subscription.IsDynamic)
                {
                    // 解析动态处理器
                    if (scope.ResolveOptional(subscription.HandlerType) is not IDynamicIntegrationEventHandler handler) continue;
                    // 将消息解析为JsonDocument类型，用于动态处理
                    using dynamic eventData = JsonDocument.Parse(message);
                    await Task.Yield();
                    await handler.Handle(eventData);
                }
                else // 静态类型事件处理
                {
                    var handler = scope.ResolveOptional(subscription.HandlerType);
                    if (handler == null) continue;
                    // 根据事件名称获取事件的具体类型
                    var eventType = _subsManager.GetEventTypeByName(eventName);
                    // 将Json字符串反序列化为集成事件对象
                    var integrationEvent = JsonSerializer.Deserialize(message,
                        eventType,
                        new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                    // 通过反射获取具体事件处理器接口的Handle方法
                    var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);

                    await Task.Yield();
                    // 调用处理器的Handle方法执行事件处理逻辑
                    await (Task)concreteType.GetMethod("Handle").Invoke(handler, new object[] { integrationEvent });
                }
            }
        }
        else
        {
            // 当不存在任何订阅时记录警告信息
            _logger.LogWarning("No subscription for RabbitMQ event: {EventName}", eventName);
        }
    }
}
