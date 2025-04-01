namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.Behaviors;

using Microsoft.Extensions.Logging;

public class TransactionBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    // 日志记录器，用于记录事务的开始、提交及异常
    private readonly ILogger<TransactionBehaviour<TRequest, TResponse>> _logger;
    // 数据库上下文，包含事务的相关操作
    private readonly OrderingContext _dbContext;
    // 集成事件服务，用于发布事件消息
    private readonly IOrderingIntegrationEventService _orderingIntegrationEventService;

    // 构造函数，初始化所需的依赖项
    public TransactionBehaviour(OrderingContext dbContext,
        IOrderingIntegrationEventService orderingIntegrationEventService,
        ILogger<TransactionBehaviour<TRequest, TResponse>> logger)
    {
        // 如果传入的 dbContext 为 null，则抛出异常
        _dbContext = dbContext ?? throw new ArgumentException(nameof(OrderingContext));
        // 如果传入的 orderingIntegrationEventService 为 null，则抛出异常
        _orderingIntegrationEventService = orderingIntegrationEventService ?? throw new ArgumentException(nameof(orderingIntegrationEventService));
        // 如果传入的 logger 为 null，则抛出异常
        _logger = logger ?? throw new ArgumentException(nameof(ILogger));
    }

    // Handle 方法负责包裹请求调用，添加事务处理逻辑
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // 初始化响应变量
        var response = default(TResponse);
        // 获取请求的类型名称（用于日志记录）
        var typeName = request.GetGenericTypeName();

        try
        {
            // 如果当前数据库上下文中已经存在活动事务，则直接执行下一个处理程序
            if (_dbContext.HasActiveTransaction)
            {
                return await next();
            }

            // 创建执行策略，用于处理重试逻辑（例如遇到瞬变故障时）
            var strategy = _dbContext.Database.CreateExecutionStrategy();

            // 使用执行策略执行事务操作
            await strategy.ExecuteAsync(async () =>
            {
                Guid transactionId;
                // 开始一个新的数据库事务，并使用 await using 确保事务资源正确释放
                await using var transaction = await _dbContext.BeginTransactionAsync();
                // 使用 LogContext 标记事务上下文，便于追踪日志记录
                using (LogContext.PushProperty("TransactionContext", transaction.TransactionId))
                {
                    // 记录开始事务日志
                    _logger.LogInformation("----- Begin transaction {TransactionId} for {CommandName} ({@Command})",
                        transaction.TransactionId, typeName, request);

                    // 执行下一个处理程序，并获取响应
                    response = await next();

                    // 记录提交事务日志
                    _logger.LogInformation("----- Commit transaction {TransactionId} for {CommandName}",
                        transaction.TransactionId, typeName);

                    // 提交事务
                    await _dbContext.CommitTransactionAsync(transaction);

                    // 保存事务 ID 供后续发布集成事件时使用
                    transactionId = transaction.TransactionId;
                }

                // 根据事务 ID 发布相关集成事件
                await _orderingIntegrationEventService.PublishEventsThroughEventBusAsync(transactionId);
            });

            // 返回响应结果
            return response;
        }
        // 捕获异常并记录错误日志后重新抛出异常
        catch (Exception ex)
        {
            _logger.LogError(ex, "ERROR Handling transaction for {CommandName} ({@Command})", typeName, request);
            throw;
        }
    }
}
