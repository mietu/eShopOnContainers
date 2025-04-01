namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.DomainEventHandlers.BuyerAndPaymentMethodVerified;

// 实现 INotificationHandler 接口用于处理领域事件
public class UpdateOrderWhenBuyerAndPaymentMethodVerifiedDomainEventHandler
                : INotificationHandler<BuyerAndPaymentMethodVerifiedDomainEvent>
{
    // 注入的订单仓储接口，负责获取和更新订单数据
    private readonly IOrderRepository _orderRepository;
    // 注入的日志工厂，用于创建日志记录器
    private readonly ILoggerFactory _logger;

    // 构造函数注入依赖项，并进行空值检查
    public UpdateOrderWhenBuyerAndPaymentMethodVerifiedDomainEventHandler(
        IOrderRepository orderRepository, ILoggerFactory logger)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // 当买家和支付方式被验证或创建时，调用此方法更新订单中的 BuyerId 和 PaymentId
    public async Task Handle(BuyerAndPaymentMethodVerifiedDomainEvent buyerPaymentMethodVerifiedEvent, CancellationToken cancellationToken)
    {
        // 从仓储获取需要更新的订单
        var orderToUpdate = await _orderRepository.GetAsync(buyerPaymentMethodVerifiedEvent.OrderId);

        // 设置订单的买家ID
        orderToUpdate.SetBuyerId(buyerPaymentMethodVerifiedEvent.Buyer.Id);
        // 设置订单的支付方式ID
        orderToUpdate.SetPaymentId(buyerPaymentMethodVerifiedEvent.Payment.Id);

        // 使用日志记录更新操作的trace信息
        _logger.CreateLogger<UpdateOrderWhenBuyerAndPaymentMethodVerifiedDomainEventHandler>()
            .LogTrace("Order with Id: {OrderId} has been successfully updated with a payment method {PaymentMethod} ({Id})",
                buyerPaymentMethodVerifiedEvent.OrderId, nameof(buyerPaymentMethodVerifiedEvent.Payment), buyerPaymentMethodVerifiedEvent.Payment.Id);
    }
}
