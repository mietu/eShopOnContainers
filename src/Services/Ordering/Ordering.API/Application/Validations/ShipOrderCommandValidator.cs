namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.Validations;

// ShipOrderCommandValidator 验证器，用于验证 ShipOrderCommand 对象的规则
public class ShipOrderCommandValidator : AbstractValidator<ShipOrderCommand>
{
    // 构造函数接收 ILogger 实例用于日志记录
    public ShipOrderCommandValidator(ILogger<ShipOrderCommandValidator> logger)
    {
        // 添加验证规则：OrderNumber 属性不能为空，
        // 如果为空，将返回错误信息 "No orderId found"
        RuleFor(order => order.OrderNumber)
            .NotEmpty()
            .WithMessage("No orderId found");

        // 记录当前实例创建的痕迹，便于调试
        logger.LogTrace("----- INSTANCE CREATED - {ClassName}", GetType().Name);
    }
}
