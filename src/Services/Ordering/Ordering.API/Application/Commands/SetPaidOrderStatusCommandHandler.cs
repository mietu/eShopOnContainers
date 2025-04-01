namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.Commands;

// 普通的命令处理器，用于更新订单支付状态
public class SetPaidOrderStatusCommandHandler : IRequestHandler<SetPaidOrderStatusCommand, bool>
{
    private readonly IOrderRepository _orderRepository;

    // 构造函数，通过依赖注入获取订单仓储对象
    public SetPaidOrderStatusCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    /// <summary>
    /// 当运送服务确认付款后触发该处理器，处理订单支付状态更新
    /// </summary>
    /// <param name="command">包含订单编号的命令对象</param>
    /// <param name="cancellationToken">任务取消标识</param>
    /// <returns>更新成功返回true，否者false</returns>
    public async Task<bool> Handle(SetPaidOrderStatusCommand command, CancellationToken cancellationToken)
    {
        // 模拟验证支付的处理时间（10秒延迟）
        await Task.Delay(10000, cancellationToken);

        // 从仓储中获取对应订单对象
        var orderToUpdate = await _orderRepository.GetAsync(command.OrderNumber);
        if (orderToUpdate == null)
        {
            // 如果订单不存在则返回false
            return false;
        }

        // 设置订单为支付状态
        orderToUpdate.SetPaidStatus();

        // 保存修改并返回操作结果
        return await _orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
    }
}


// 用于处理幂等性更新的命令处理器，防止重复请求
public class SetPaidIdentifiedOrderStatusCommandHandler : IdentifiedCommandHandler<SetPaidOrderStatusCommand, bool>
{
    // 构造函数，通过依赖注入获取IMediator、IRequestManager和ILogger
    public SetPaidIdentifiedOrderStatusCommandHandler(
        IMediator mediator,
        IRequestManager requestManager,
        ILogger<IdentifiedCommandHandler<SetPaidOrderStatusCommand, bool>> logger)
        : base(mediator, requestManager, logger)
    {
    }

    // 当检测到重复的请求时，返回默认的成功结果
    protected override bool CreateResultForDuplicateRequest()
    {
        return true;  // 忽略重复请求，认为处理成功
    }
}
