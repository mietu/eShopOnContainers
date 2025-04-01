namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.Commands;

/// <summary>
/// 提供处理重复请求（基于客户端发送的requestid）的基本实现，确保幂等性更新。
/// 其中，T为要执行操作的命令类型，R为内部命令处理返回的结果类型。
/// </summary>
/// <typeparam name="T">内部命令的类型</typeparam>
/// <typeparam name="R">内部命令执行的返回值类型</typeparam>
public class IdentifiedCommandHandler<T, R> : IRequestHandler<IdentifiedCommand<T, R>, R>
    where T : IRequest<R>
{
    // Mediator用于发送和调度命令
    private readonly IMediator _mediator;
    // 请求管理器用于检测和记录请求的唯一性，防止重复执行
    private readonly IRequestManager _requestManager;
    // 日志记录器，用于记录处理过程中的信息
    private readonly ILogger<IdentifiedCommandHandler<T, R>> _logger;

    // 构造函数，依赖注入mediator、请求管理器、日志记录器
    public IdentifiedCommandHandler(
        IMediator mediator,
        IRequestManager requestManager,
        ILogger<IdentifiedCommandHandler<T, R>> logger)
    {
        _mediator = mediator;
        _requestManager = requestManager;
        _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 当检测到重复请求时，生成默认的返回结果。
    /// 如果需要返回特定的重复请求结果，可以重写该方法。
    /// </summary>
    /// <returns>重复请求时返回的结果，默认值为default(R)</returns>
    protected virtual R CreateResultForDuplicateRequest()
    {
        return default(R);
    }

    /// <summary>
    /// 处理命令，确保相同请求ID的命令只执行一次。
    /// 如果请求ID已存在，则返回默认结果；否则创建请求记录后，继续执行内部命令。
    /// </summary>
    /// <param name="message">包含原始命令和请求ID的IdentifiedCommand对象</param>
    /// <param name="cancellationToken">取消标识，用于取消该操作</param>
    /// <returns>内部命令处理的返回结果，如果为重复请求则返回默认值</returns>
    public async Task<R> Handle(IdentifiedCommand<T, R> message, CancellationToken cancellationToken)
    {
        // 检查该请求ID是否已存在，确保命令只执行一次
        var alreadyExists = await _requestManager.ExistAsync(message.Id);
        if (alreadyExists)
        {
            // 若存在，则直接返回重复请求结果
            return CreateResultForDuplicateRequest();
        }
        else
        {
            // 创建一个新的请求记录，标记该请求已被处理
            await _requestManager.CreateRequestForCommandAsync<T>(message.Id);
            try
            {
                var command = message.Command;
                var commandName = command.GetGenericTypeName();
                var idProperty = string.Empty;
                var commandId = string.Empty;

                // 根据不同的命令类型提取对应的标识属性和标识值
                switch (command)
                {
                    case CreateOrderCommand createOrderCommand:
                        idProperty = nameof(createOrderCommand.UserId);
                        commandId = createOrderCommand.UserId;
                        break;

                    case CancelOrderCommand cancelOrderCommand:
                        idProperty = nameof(cancelOrderCommand.OrderNumber);
                        commandId = $"{cancelOrderCommand.OrderNumber}";
                        break;

                    case ShipOrderCommand shipOrderCommand:
                        idProperty = nameof(shipOrderCommand.OrderNumber);
                        commandId = $"{shipOrderCommand.OrderNumber}";
                        break;

                    default:
                        idProperty = "Id?";
                        commandId = "n/a";
                        break;
                }

                // 记录发送命令前的日志，包括命令名称、标识属性和值以及命令内容
                _logger.LogInformation(
                    "----- Sending command: {CommandName} - {IdProperty}: {CommandId} ({@Command})",
                    commandName,
                    idProperty,
                    commandId,
                    command);

                // 通过Mediator发送内部命令到对应的命令处理器
                var result = await _mediator.Send(command, cancellationToken);

                // 记录命令处理结果及相关信息日志
                _logger.LogInformation(
                    "----- Command result: {@Result} - {CommandName} - {IdProperty}: {CommandId} ({@Command})",
                    result,
                    commandName,
                    idProperty,
                    commandId,
                    command);

                return result;
            }
            catch
            {
                // 发生异常时，返回默认结果
                return default(R);
            }
        }
    }
}
