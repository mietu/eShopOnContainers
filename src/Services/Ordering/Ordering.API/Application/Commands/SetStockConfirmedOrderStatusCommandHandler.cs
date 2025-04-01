namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.Commands;

// 处理库存确认状态更新的命令处理程序
public class SetStockConfirmedOrderStatusCommandHandler : IRequestHandler<SetStockConfirmedOrderStatusCommand, bool>
{
    // 注入订单仓储接口，用于对订单数据进行操作
    private readonly IOrderRepository _orderRepository;

    // 构造函数，初始化订单仓储依赖
    public SetStockConfirmedOrderStatusCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    /// <summary>
    /// 处理库存确认命令
    /// 当库存服务确认请求时调用此方法
    /// </summary>
    /// <param name="command">股票确认的命令，包含订单编号</param>
    /// <param name="cancellationToken">取消标志</param>
    /// <returns>返回是否成功更新订单状态的布尔值</returns>
    public async Task<bool> Handle(SetStockConfirmedOrderStatusCommand command, CancellationToken cancellationToken)
    {
        // 模拟等待时间（例如库存确认过程耗时10秒）
        await Task.Delay(10000, cancellationToken);

        // 根据命令中提供的订单编号获取订单记录
        var orderToUpdate = await _orderRepository.GetAsync(command.OrderNumber);
        if (orderToUpdate == null)
        {
            // 如果找不到订单，则返回false
            return false;
        }

        // 更新订单状态为“库存已确认”
        orderToUpdate.SetStockConfirmedStatus();

        // 保存订单状态的变更，并返回操作是否成功
        return await _orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
    }
}


// 处理具备幂等性的命令处理程序
// 用于确保相同的请求ID不会被重复处理
public class SetStockConfirmedOrderStatusIdenfifiedCommandHandler : IdentifiedCommandHandler<SetStockConfirmedOrderStatusCommand, bool>
{
    // 构造函数：注入mediator、请求管理器以及日志记录器
    public SetStockConfirmedOrderStatusIdenfifiedCommandHandler(
        IMediator mediator,
        IRequestManager requestManager,
        ILogger<IdentifiedCommandHandler<SetStockConfirmedOrderStatusCommand, bool>> logger)
        : base(mediator, requestManager, logger)
    {
    }

    // 当检测到重复请求时，返回true作为默认结果，表示重复请求被忽略
    protected override bool CreateResultForDuplicateRequest()
    {
        return true;
    }
}
