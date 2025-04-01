namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.Commands;

// 处理取消订单的命令：
// 实现了 IRequestHandler 接口，用于处理 CancelOrderCommand 返回类型为 bool 的命令
public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, bool>
{
    // 订单仓储，用于操作订单持久化
    private readonly IOrderRepository _orderRepository;

    // 构造函数注入订单仓储依赖
    public CancelOrderCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    /// <summary>
    /// 处理取消订单命令的业务方法
    /// 当客户在应用程序中执行取消订单操作时调用此方法
    /// </summary>
    /// <param name="command">取消订单的命令，包含订单号码</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>返回一个布尔值，表示操作是否成功</returns>
    public async Task<bool> Handle(CancelOrderCommand command, CancellationToken cancellationToken)
    {
        // 根据传入的订单号码查找要取消的订单
        var orderToUpdate = await _orderRepository.GetAsync(command.OrderNumber);
        // 如果未找到订单，返回 false
        if (orderToUpdate == null)
        {
            return false;
        }

        // 设置订单为取消状态
        orderToUpdate.SetCancelledStatus();
        // 保存更改，返回保存操作结果
        return await _orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
    }
}


// 专门用于确保命令幂等性的处理器
// 当重复请求被识别时，此处理器将返回默认的结果
public class CancelOrderIdentifiedCommandHandler : IdentifiedCommandHandler<CancelOrderCommand, bool>
{
    // 构造函数，注入 Mediator、请求管理器和日志记录器
    public CancelOrderIdentifiedCommandHandler(
        IMediator mediator,
        IRequestManager requestManager,
        ILogger<IdentifiedCommandHandler<CancelOrderCommand, bool>> logger)
        : base(mediator, requestManager, logger)
    {
    }

    // 当检测到重复请求时，返回 true 表示重复的取消订单操作被忽略
    protected override bool CreateResultForDuplicateRequest()
    {
        return true; // 忽略重复的请求，不再重复处理取消订单命令
    }
}
