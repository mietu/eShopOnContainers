namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.DomainEventHandlers.OrderStartedEvent;

// 该类用于处理 OrderStartedDomainEvent 领域事件，
// 主要作用是在订单开始时验证或者添加买家（Buyer）聚合根
public class ValidateOrAddBuyerAggregateWhenOrderStartedDomainEventHandler : INotificationHandler<OrderStartedDomainEvent>
{
    private readonly ILoggerFactory _logger;
    private readonly IBuyerRepository _buyerRepository;
    private readonly IIdentityService _identityService;
    private readonly IOrderingIntegrationEventService _orderingIntegrationEventService;

    // 构造函数注入相关依赖，确保依赖不为空
    public ValidateOrAddBuyerAggregateWhenOrderStartedDomainEventHandler(
        ILoggerFactory logger,
        IBuyerRepository buyerRepository,
        IIdentityService identityService,
        IOrderingIntegrationEventService orderingIntegrationEventService)
    {
        _buyerRepository = buyerRepository ?? throw new ArgumentNullException(nameof(buyerRepository));
        _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
        _orderingIntegrationEventService = orderingIntegrationEventService ?? throw new ArgumentNullException(nameof(orderingIntegrationEventService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // 处理 OrderStartedDomainEvent 领域事件
    public async Task Handle(OrderStartedDomainEvent orderStartedEvent, CancellationToken cancellationToken)
    {
        // 如果 CardTypeId 为0，则默认使用1；否则使用传入的CardTypeId
        var cardTypeId = (orderStartedEvent.CardTypeId != 0) ? orderStartedEvent.CardTypeId : 1;

        // 通过仓储查找是否存在对应的买家
        var buyer = await _buyerRepository.FindAsync(orderStartedEvent.UserId);
        bool buyerOriginallyExisted = (buyer == null) ? false : true;

        // 如果买家不存在，则创建新的 Buyer 实例
        if (!buyerOriginallyExisted)
        {
            buyer = new Buyer(orderStartedEvent.UserId, orderStartedEvent.UserName);
        }

        // 验证或添加支付方式，关联订单信息
        buyer.VerifyOrAddPaymentMethod(
            cardTypeId,
            $"Payment Method on {DateTime.UtcNow}",  // 别名 —— 包含当前 UTC 时间
            orderStartedEvent.CardNumber,
            orderStartedEvent.CardSecurityNumber,
            orderStartedEvent.CardHolderName,
            orderStartedEvent.CardExpiration,
            orderStartedEvent.Order.Id);

        // 根据买家是否原本存在，选择更新还是添加买家
        var buyerUpdated = buyerOriginallyExisted ?
            _buyerRepository.Update(buyer) :
            _buyerRepository.Add(buyer);

        // 保存买家和支付方式的更改到数据库
        await _buyerRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        // 创建订单状态变更集成事件（订单状态更改为已提交）
        var orderStatusChangedToSubmittedIntegrationEvent = new OrderStatusChangedToSubmittedIntegrationEvent(
            orderStartedEvent.Order.Id,
            orderStartedEvent.Order.OrderStatus.Name,
            buyer.Name);
        // 将集成事件添加并保存到事件总线上
        await _orderingIntegrationEventService.AddAndSaveEventAsync(orderStatusChangedToSubmittedIntegrationEvent);

        // 打印日志跟踪信息
        _logger.CreateLogger<ValidateOrAddBuyerAggregateWhenOrderStartedDomainEventHandler>()
            .LogTrace("Buyer {BuyerId} and related payment method were validated or updated for orderId: {OrderId}.",
                buyerUpdated.Id, orderStartedEvent.Order.Id);
    }
}
