namespace Microsoft.eShopOnContainers.BuildingBlocks.EventBusRabbitMQ;

// RabbitMQ的持久连接实现，用于确保应用程序与RabbitMQ服务器保持连接
public class DefaultRabbitMQPersistentConnection : IRabbitMQPersistentConnection
{
    // 用于创建连接的工厂
    private readonly IConnectionFactory _connectionFactory;
    // 日志记录器
    private readonly ILogger<DefaultRabbitMQPersistentConnection> _logger;
    // 重试次数
    private readonly int _retryCount;
    // RabbitMQ连接对象
    private IConnection _connection;
    // 标识该对象是否已被释放
    public bool Disposed;

    // 同步锁对象，用于线程安全
    readonly object _syncRoot = new();

    // 构造函数，注入依赖
    public DefaultRabbitMQPersistentConnection(IConnectionFactory connectionFactory, ILogger<DefaultRabbitMQPersistentConnection> logger, int retryCount = 5)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _retryCount = retryCount;
    }

    // 判断连接是否有效且未释放
    public bool IsConnected => _connection is { IsOpen: true } && !Disposed;

    // 创建并返回模型(Channel)，用于发送或接收消息
    public IModel CreateModel()
    {
        if (!IsConnected)
        {
            throw new InvalidOperationException("No RabbitMQ connections are available to perform this action");
        }

        return _connection.CreateModel();
    }

    // 释放资源并注销事件
    public void Dispose()
    {
        if (Disposed) return;

        Disposed = true;

        try
        {
            // 注销事件监听
            _connection.ConnectionShutdown -= OnConnectionShutdown;
            _connection.CallbackException -= OnCallbackException;
            _connection.ConnectionBlocked -= OnConnectionBlocked;
            // 释放连接资源
            _connection.Dispose();
        }
        catch (IOException ex)
        {
            _logger.LogCritical(ex.ToString());
        }
    }

    // 尝试建立连接
    public bool TryConnect()
    {
        _logger.LogInformation("RabbitMQ Client is trying to connect");

        lock (_syncRoot)
        {
            // 使用重试策略进行连接重试，捕获SocketException和BrokerUnreachableException异常
            var policy = RetryPolicy.Handle<SocketException>()
                .Or<BrokerUnreachableException>()
                .WaitAndRetry(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                {
                    // 当连接异常时记录警告日志
                    _logger.LogWarning(ex, "RabbitMQ Client could not connect after {TimeOut}s ({ExceptionMessage})", $"{time.TotalSeconds:n1}", ex.Message);
                }
            );

            // 执行建立连接的操作
            policy.Execute(() =>
            {
                _connection = _connectionFactory.CreateConnection();
            });

            if (IsConnected)
            {
                // 注册连接事件回调
                _connection.ConnectionShutdown += OnConnectionShutdown;
                _connection.CallbackException += OnCallbackException;
                _connection.ConnectionBlocked += OnConnectionBlocked;

                _logger.LogInformation("RabbitMQ Client acquired a persistent connection to '{HostName}' and is subscribed to failure events", _connection.Endpoint.HostName);

                return true;
            }
            else
            {
                _logger.LogCritical("FATAL ERROR: RabbitMQ connections could not be created and opened");

                return false;
            }
        }
    }

    // 当RabbitMQ连接被阻塞时调用
    private void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
    {
        if (Disposed) return;

        _logger.LogWarning("A RabbitMQ connection is shutdown. Trying to re-connect...");

        // 尝试重连
        TryConnect();
    }

    // 当RabbitMQ回调发生异常时调用
    void OnCallbackException(object sender, CallbackExceptionEventArgs e)
    {
        if (Disposed) return;

        _logger.LogWarning("A RabbitMQ connection throw exception. Trying to re-connect...");

        // 尝试重连
        TryConnect();
    }

    // 当RabbitMQ连接关闭时调用
    void OnConnectionShutdown(object sender, ShutdownEventArgs reason)
    {
        if (Disposed) return;

        _logger.LogWarning("A RabbitMQ connection is on shutdown. Trying to re-connect...");

        // 尝试重连
        TryConnect();
    }
}
