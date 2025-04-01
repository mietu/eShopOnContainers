namespace Microsoft.eShopOnContainers.Services.Ordering.Domain.AggregatesModel.OrderAggregate;

// Order聚合根，封装了订单业务逻辑及状态转换
public class Order : Entity, IAggregateRoot
{
    // 订单创建时间（内部字段）
    private DateTime _orderDate;

    // 送货地址（值对象模式，EF Core 2.0的owned entity）
    public Address Address { get; private set; }

    // 买家ID（只读属性，通过私有字段保存）
    public int? GetBuyerId => _buyerId;
    private int? _buyerId;

    // 订单状态（对应业务状态，枚举封装）
    public OrderStatus OrderStatus { get; private set; }
    private int _orderStatusId;

    // 订单描述信息（用于描述订单状态传递信息）
    private string _description;

    // 是否为草稿订单（草稿订单可能还未提交）
    private bool _isDraft;

    // 内部订单项集合，采用私有字段封装，防止外部直接操作
    private readonly List<OrderItem> _orderItems;
    public IReadOnlyCollection<OrderItem> OrderItems => _orderItems;

    // 支付方式ID（可选）
    private int? _paymentMethodId;

    // 创建一个新的草稿订单
    public static Order NewDraft()
    {
        var order = new Order();
        order._isDraft = true;
        return order;
    }

    // 无参构造函数，初始化内部集合
    protected Order()
    {
        _orderItems = new List<OrderItem>();
        _isDraft = false;
    }

    // 带参数构造函数，创建订单并初始化相关数据，同时添加订单启动事件
    public Order(string userId, string userName, Address address, int cardTypeId, string cardNumber, string cardSecurityNumber,
            string cardHolderName, DateTime cardExpiration, int? buyerId = null, int? paymentMethodId = null) : this()
    {
        _buyerId = buyerId;
        _paymentMethodId = paymentMethodId;
        _orderStatusId = OrderStatus.Submitted.Id; // 初始状态为Submitted
        _orderDate = DateTime.UtcNow;
        Address = address;

        // 添加订单启动领域事件，便于后续处理（如发送通知、日志记录等）
        AddOrderStartedDomainEvent(userId, userName, cardTypeId, cardNumber,
                                    cardSecurityNumber, cardHolderName, cardExpiration);
    }

    // 添加订单项, 保证聚合内数据一致性；如果订单项已存在，更新数量和折扣
    public void AddOrderItem(int productId, string productName, decimal unitPrice, decimal discount, string pictureUrl, int units = 1)
    {
        var existingOrderForProduct = _orderItems
            .Where(o => o.ProductId == productId)
            .SingleOrDefault();

        if (existingOrderForProduct != null)
        {
            // 如果已有该产品订单项，更新折扣（取较大折扣）及增加数量
            if (discount > existingOrderForProduct.GetCurrentDiscount())
            {
                existingOrderForProduct.SetNewDiscount(discount);
            }
            existingOrderForProduct.AddUnits(units);
        }
        else
        {
            // 如果不存在，则添加新的订单项
            var orderItem = new OrderItem(productId, productName, unitPrice, discount, pictureUrl, units);
            _orderItems.Add(orderItem);
        }
    }

    // 设置支付方式ID
    public void SetPaymentId(int id)
    {
        _paymentMethodId = id;
    }

    // 设置买家ID
    public void SetBuyerId(int id)
    {
        _buyerId = id;
    }

    // 设置状态为AwaitingValidation，同时触发相应领域事件
    public void SetAwaitingValidationStatus()
    {
        if (_orderStatusId == OrderStatus.Submitted.Id)
        {
            AddDomainEvent(new OrderStatusChangedToAwaitingValidationDomainEvent(Id, _orderItems));
            _orderStatusId = OrderStatus.AwaitingValidation.Id;
        }
    }

    // 设置状态为StockConfirmed，同时记录状态信息并触发领域事件
    public void SetStockConfirmedStatus()
    {
        if (_orderStatusId == OrderStatus.AwaitingValidation.Id)
        {
            AddDomainEvent(new OrderStatusChangedToStockConfirmedDomainEvent(Id));
            _orderStatusId = OrderStatus.StockConfirmed.Id;
            _description = "All the items were confirmed with available stock.";
        }
    }

    // 设置状态为Paid，并触发支付成功的领域事件，同时记录支付描述
    public void SetPaidStatus()
    {
        if (_orderStatusId == OrderStatus.StockConfirmed.Id)
        {
            AddDomainEvent(new OrderStatusChangedToPaidDomainEvent(Id, OrderItems));
            _orderStatusId = OrderStatus.Paid.Id;
            _description = "The payment was performed at a simulated \"American Bank checking bank account ending on XX35071\"";
        }
    }

    // 设置状态为Shipped，只能在订单支付成功后进行，否则抛异常；并触发已发货领域事件
    public void SetShippedStatus()
    {
        if (_orderStatusId != OrderStatus.Paid.Id)
        {
            StatusChangeException(OrderStatus.Shipped);
        }

        _orderStatusId = OrderStatus.Shipped.Id;
        _description = "The order was shipped.";
        AddDomainEvent(new OrderShippedDomainEvent(this));
    }

    // 设置状态为Cancelled；对于已支付或已发货的订单，状态修改无效抛异常；并触发取消领域事件
    public void SetCancelledStatus()
    {
        if (_orderStatusId == OrderStatus.Paid.Id ||
            _orderStatusId == OrderStatus.Shipped.Id)
        {
            StatusChangeException(OrderStatus.Cancelled);
        }

        _orderStatusId = OrderStatus.Cancelled.Id;
        _description = $"The order was cancelled.";
        AddDomainEvent(new OrderCancelledDomainEvent(this));
    }

    // 当库存不足时取消订单，根据拒绝的订单项更新描述信息
    public void SetCancelledStatusWhenStockIsRejected(IEnumerable<int> orderStockRejectedItems)
    {
        if (_orderStatusId == OrderStatus.AwaitingValidation.Id)
        {
            _orderStatusId = OrderStatus.Cancelled.Id;

            // 根据传入的产品ID获取对应的产品名称列表
            var itemsStockRejectedProductNames = OrderItems
                .Where(c => orderStockRejectedItems.Contains(c.ProductId))
                .Select(c => c.GetOrderItemProductName());

            var itemsStockRejectedDescription = string.Join(", ", itemsStockRejectedProductNames);
            _description = $"The product items don't have stock: ({itemsStockRejectedDescription}).";
        }
    }

    // 内部方法：添加订单启动领域事件
    private void AddOrderStartedDomainEvent(string userId, string userName, int cardTypeId, string cardNumber,
            string cardSecurityNumber, string cardHolderName, DateTime cardExpiration)
    {
        var orderStartedDomainEvent = new OrderStartedDomainEvent(
            this, userId, userName, cardTypeId, cardNumber,
            cardSecurityNumber, cardHolderName, cardExpiration);

        this.AddDomainEvent(orderStartedDomainEvent);
    }

    // 内部方法：状态变更异常，抛出对应业务异常信息
    private void StatusChangeException(OrderStatus orderStatusToChange)
    {
        throw new OrderingDomainException($"Is not possible to change the order status from {OrderStatus.Name} to {orderStatusToChange.Name}.");
    }

    // 计算订单总金额，通过订单项数量和单价进行汇总
    public decimal GetTotal()
    {
        return _orderItems.Sum(o => o.GetUnits() * o.GetUnitPrice());
    }
}
