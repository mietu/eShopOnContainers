namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.Behaviors;
/// <summary>
/// 日志处理中间件，用于记录请求和响应的日志信息
/// </summary>
/// <typeparam name="TRequest">请求类型</typeparam>
/// <typeparam name="TResponse">响应类型</typeparam>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    // 日志记录器，用于输出信息
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    // 构造函数，注入日志记录器
    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        => _logger = logger;

    /// <summary>
    /// 处理请求的管道方法
    /// </summary>
    /// <param name="request">当前请求</param>
    /// <param name="next">下一个处理委托</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>处理后的响应</returns>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // 记录开始处理请求的日志信息
        _logger.LogInformation("----- Handling command {CommandName} ({@Command})", request.GetGenericTypeName(), request);

        // 调用下一个处理器并等待其处理结果
        var response = await next();

        // 记录处理完成后的响应日志信息
        _logger.LogInformation("----- Command {CommandName} handled - response: {@Response}", request.GetGenericTypeName(), response);

        // 返回处理结果
        return response;
    }
}
