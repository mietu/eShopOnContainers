namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.Validations;

/// <summary>
/// 用于验证标识命令的有效性。此验证器针对 IdentifiedCommand，其中封装了 CreateOrderCommand 命令和一个布尔返回值。
/// </summary>
public class IdentifiedCommandValidator : AbstractValidator<IdentifiedCommand<CreateOrderCommand, bool>>
{
    /// <summary>
    /// 构造函数，设置命令验证规则，并记录实例创建日志。
    /// </summary>
    /// <param name="logger">用于记录日志的 ILogger 实例。</param>
    public IdentifiedCommandValidator(ILogger<IdentifiedCommandValidator> logger)
    {
        // 验证命令的 Id 字段不为空
        RuleFor(command => command.Id).NotEmpty();

        // 记录跟踪日志，表明当前验证器实例已经创建
        logger.LogTrace("----- INSTANCE CREATED - {ClassName}", GetType().Name);
    }
}
