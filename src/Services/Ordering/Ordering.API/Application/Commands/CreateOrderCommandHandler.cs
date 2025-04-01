namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.Commands;

using Microsoft.eShopOnContainers.Services.Ordering.Domain.AggregatesModel.OrderAggregate;

// CommandHandler 用于处理创建订单的命令
public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, bool>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IIdentityService _identityService;
    private readonly IMediator _mediator;
    private readonly IOrderingIntegrationEventService _orderingIntegrationEventService;
    private readonly ILogger<CreateOrderCommandHandler> _logger;

    // 构造函数：使用依赖注入获取所需的服务实例
    public CreateOrderCommandHandler(IMediator mediator,
        IOrderingIntegrationEventService orderingIntegrationEventService,
        IOrderRepository orderRepository,
        IIdentityService identityService,
        ILogger<CreateOrderCommandHandler> logger)
    {
        // 检查并保存各个依赖服务实例，若有任何一个为空则抛出异常
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _orderingIntegrationEventService = orderingIntegrationEventService ?? throw new ArgumentNullException(nameof(orderingIntegrationEventService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // 处理创建订单的命令
    public async Task<bool> Handle(CreateOrderCommand message, CancellationToken cancellationToken)
    {
        // 创建订单开始集成事件，用于后续清理购物篮等操作
        var orderStartedIntegrationEvent = new OrderStartedIntegrationEvent(message.UserId);
        await _orderingIntegrationEventService.AddAndSaveEventAsync(orderStartedIntegrationEvent);

        // 构造地址对象，通过传入命令中的地址信息
        var address = new Address(message.Street, message.City, message.State, message.Country, message.ZipCode);
        // 创建订单对象，包含用户信息、地址和支付卡信息
        var order = new Order(message.UserId, message.UserName, address, message.CardTypeId,
                              message.CardNumber, message.CardSecurityNumber, message.CardHolderName, message.CardExpiration);

        // 遍历订单项，将每一项添加到订单中
        foreach (var item in message.OrderItems)
        {
            order.AddOrderItem(item.ProductId, item.ProductName, item.UnitPrice, item.Discount, item.PictureUrl, item.Units);
        }

        // 记录日志，便于调试和追踪订单创建过程
        _logger.LogInformation("----- Creating Order - Order: {@Order}", order);

        // 将订单添加到仓储中
        _orderRepository.Add(order);

        // 保存更改，返回保存操作的结果
        return await _orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
    }
}


// IdentifiedCommandHandler 用于处理具备幂等性的创建订单命令
public class CreateOrderIdentifiedCommandHandler : IdentifiedCommandHandler<CreateOrderCommand, bool>
{
    // 构造函数：通过依赖注入接收 IMediator、IRequestManager 和 ILogger
    public CreateOrderIdentifiedCommandHandler(
        IMediator mediator,
        IRequestManager requestManager,
        ILogger<IdentifiedCommandHandler<CreateOrderCommand, bool>> logger)
        : base(mediator, requestManager, logger)
    {
    }

    // 当检测到重复请求时，返回固定结果 true
    protected override bool CreateResultForDuplicateRequest()
    {
        return true; // 忽略重复的创建订单请求
    }
}
