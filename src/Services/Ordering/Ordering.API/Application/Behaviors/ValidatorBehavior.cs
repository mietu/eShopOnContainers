namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.Behaviors;

// ValidatorBehavior 类实现了 IPipelineBehavior 接口，用于在命令处理管道中执行验证逻辑。
public class ValidatorBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly ILogger<ValidatorBehavior<TRequest, TResponse>> _logger;
    private readonly IEnumerable<IValidator<TRequest>> _validators;  // 集合中包含所有对 TRequest 进行验证的验证器

    // 构造函数注入验证器集合和日志记录器
    public ValidatorBehavior(IEnumerable<IValidator<TRequest>> validators, ILogger<ValidatorBehavior<TRequest, TResponse>> logger)
    {
        _validators = validators;
        _logger = logger;
    }

    // Handle 方法负责执行验证逻辑，并在成功验证后继续调用下一个处理环节
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // 获取请求的类型名称，用于日志记录
        var typeName = request.GetGenericTypeName();

        // 记录日志，说明正在验证此命令
        _logger.LogInformation("----- Validating command {CommandType}", typeName);

        // 对每个验证器执行验证，并收集所有错误
        var failures = _validators
            .Select(v => v.Validate(request))   // 执行验证，返回验证结果
            .SelectMany(result => result.Errors)  // 将所有验证错误合并为一个集合
            .Where(error => error != null)        // 筛选出非空的错误
            .ToList();

        // 如果找到任何验证错误，则记录警告并抛出异常
        if (failures.Any())
        {
            _logger.LogWarning("Validation errors - {CommandType} - Command: {@Command} - Errors: {@ValidationErrors}", typeName, request, failures);

            throw new Domain.Exceptions.OrderingDomainException(
                $"Command Validation Errors for type {typeof(TRequest).Name}",
                new ValidationException("Validation exception", failures));
        }

        // 如果没有验证错误，则继续调用管道中的下一个处理器
        return await next();
    }
}
