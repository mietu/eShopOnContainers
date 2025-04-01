namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.Commands;

// ICommandHandler：处理设置库存拒绝后的订单状态请求。
public class SetStockRejectedOrderStatusCommandHandler : IRequestHandler<SetStockRejectedOrderStatusCommand, bool>
{
    // 注入订单仓储，用于获取和更新订单信息
    private readonly IOrderRepository _orderRepository;

    // 构造函数，依赖注入 IOrderRepository 实例
    public SetStockRejectedOrderStatusCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    /// <summary>
    /// 处理库存服务拒绝订单时的命令，将订单状态设为已取消。
    /// 包含模拟延时以代表实际工作耗时。
    /// </summary>
    /// <param name="command">包含订单号和库存项信息的命令</param>
    /// <param name="cancellationToken">取消标识</param>
    /// <returns>如果更新成功返回true，否则返回false</returns>
    public async Task<bool> Handle(SetStockRejectedOrderStatusCommand command, CancellationToken cancellationToken)
    {
        // 模拟工作处理时间，等待10秒
        await Task.Delay(10000, cancellationToken);

        // 根据订单号获取订单
        var orderToUpdate = await _orderRepository.GetAsync(command.OrderNumber);
        if (orderToUpdate == null)
        {
            // 如果订单不存在，则返回false
            return false;
        }

        // 调用订单域模型方法，根据库存项更新订单状态为取消状态
        orderToUpdate.SetCancelledStatusWhenStockIsRejected(command.OrderStockItems);

        // 保存变更到数据库，返回保存结果
        return await _orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
    }
}


// 处理带有幂等性支持的命令，防止重复请求导致重复处理
public class SetStockRejectedOrderStatusIdenfifiedCommandHandler : IdentifiedCommandHandler<SetStockRejectedOrderStatusCommand, bool>
{
    // 构造函数，注入 IMediator、IRequestManager 和 ILogger 实例
    public SetStockRejectedOrderStatusIdenfifiedCommandHandler(
        IMediator mediator,
        IRequestManager requestManager,
        ILogger<IdentifiedCommandHandler<SetStockRejectedOrderStatusCommand, bool>> logger)
        : base(mediator, requestManager, logger)
    {
    }

    /// <summary>
    /// 当检测到重复请求时，返回默认的处理结果
    /// </summary>
    /// <returns>重复请求的默认返回值，此处为 true，表示忽略重复请求</returns>
    protected override bool CreateResultForDuplicateRequest()
    {
        return true; // 忽略重复请求，直接返回成功标识
    }
}
