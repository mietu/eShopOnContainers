namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.Commands;

// 处理 SetAwaitingValidationOrderStatusCommand 命令的处理程序
public class SetAwaitingValidationOrderStatusCommandHandler : IRequestHandler<SetAwaitingValidationOrderStatusCommand, bool>
{
    // 注入订单仓储，用于操作订单实体
    private readonly IOrderRepository _orderRepository;

    // 构造函数，通过依赖注入初始化仓储
    public SetAwaitingValidationOrderStatusCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    /// <summary>
    /// 当宽限期结束后，此命令被调用，用于更新订单状态为 AwaitingValidation
    /// </summary>
    /// <param name="command">包含订单编号的命令对象</param>
    /// <param name="cancellationToken">用于取消操作的标识</param>
    /// <returns>如果订单状态更新且保存成功返回 true，否则返回 false</returns>
    public async Task<bool> Handle(SetAwaitingValidationOrderStatusCommand command, CancellationToken cancellationToken)
    {
        // 根据订单编号从仓储中获取订单实体
        var orderToUpdate = await _orderRepository.GetAsync(command.OrderNumber);
        if (orderToUpdate == null)
        {
            // 当订单不存在时，返回 false 表示操作失败
            return false;
        }

        // 调用订单实体方法设置订单状态为 AwaitingValidation
        orderToUpdate.SetAwaitingValidationStatus();
        // 保存更新到数据库，并返回保存操作结果
        return await _orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
    }
}


// 使用 IdentifiedCommand 模式实现命令的幂等性处理，防止重复请求执行相同操作
public class SetAwaitingValidationIdentifiedOrderStatusCommandHandler : IdentifiedCommandHandler<SetAwaitingValidationOrderStatusCommand, bool>
{
    // 构造函数，将 Mediator、请求管理器和日志记录器注入基础类
    public SetAwaitingValidationIdentifiedOrderStatusCommandHandler(
        IMediator mediator,
        IRequestManager requestManager,
        ILogger<IdentifiedCommandHandler<SetAwaitingValidationOrderStatusCommand, bool>> logger)
        : base(mediator, requestManager, logger)
    {
    }

    // 重写处理重复请求时返回的结果
    // 当检测到重复请求时，返回 true 表示忽略重复请求，认为订单状态已成功更新
    protected override bool CreateResultForDuplicateRequest()
    {
        return true;
    }
}
