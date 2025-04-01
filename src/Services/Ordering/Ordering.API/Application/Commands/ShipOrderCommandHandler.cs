namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.Commands;

// ShipOrderCommandHandler 负责处理发货命令，当管理员发起发货请求时执行发货操作
public class ShipOrderCommandHandler : IRequestHandler<ShipOrderCommand, bool>
{
    // 注入订单仓储接口，用于查询和更新订单
    private readonly IOrderRepository _orderRepository;

    // 构造函数，使用依赖注入传入订单仓储
    public ShipOrderCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    /// <summary>
    /// 处理发货命令。首先根据命令中提供的订单编号获取订单。
    /// 如果订单存在，则调用订单的 SetShippedStatus 方法设置订单状态为已发货，
    /// 最后保存更改并返回保存结果；如果订单不存在，则返回 false。
    /// </summary>
    /// <param name="command">包含订单编号的发货命令</param>
    /// <param name="cancellationToken">取消标记</param>
    /// <returns>如果操作成功返回 true，否则返回 false</returns>
    public async Task<bool> Handle(ShipOrderCommand command, CancellationToken cancellationToken)
    {
        // 根据命令中的 OrderNumber 获取订单
        var orderToUpdate = await _orderRepository.GetAsync(command.OrderNumber);
        // 如果订单不存在，则直接返回 false
        if (orderToUpdate == null)
        {
            return false;
        }

        // 设置订单状态为已发货
        orderToUpdate.SetShippedStatus();
        // 保存订单状态更改，并返回保存操作结果
        return await _orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
    }
}


// ShipOrderIdentifiedCommandHandler 用于处理具有幂等性的发货命令
// 通过继承 IdentifiedCommandHandler 类，确保相同请求ID的命令只执行一次
public class ShipOrderIdentifiedCommandHandler : IdentifiedCommandHandler<ShipOrderCommand, bool>
{
    // 构造函数，注入mediator、请求管理器和日志记录器
    public ShipOrderIdentifiedCommandHandler(
        IMediator mediator,
        IRequestManager requestManager,
        ILogger<IdentifiedCommandHandler<ShipOrderCommand, bool>> logger)
        : base(mediator, requestManager, logger)
    {
    }

    // 当检测到重复请求时，返回 true 表示忽略重复执行发货操作
    protected override bool CreateResultForDuplicateRequest()
    {
        // 如果重复请求，则返回 true（表示订单已发货或无需再次处理）
        return true;
    }
}
