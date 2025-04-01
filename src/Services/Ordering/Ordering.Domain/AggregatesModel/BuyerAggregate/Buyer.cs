namespace Microsoft.eShopOnContainers.Services.Ordering.Domain.AggregatesModel.BuyerAggregate;

/// <summary>
/// Buyer 类表示订单领域中的买家聚合根。
/// 该类维护了买家的身份标识、姓名与支付方式列表。
/// </summary>
public class Buyer : Entity, IAggregateRoot
{
    /// <summary>
    /// 买家唯一身份标识符（通常对应外部用户系统）。
    /// </summary>
    public string IdentityGuid { get; private set; }

    /// <summary>
    /// 买家姓名。
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// 存储买家所使用的支付方式集合。
    /// </summary>
    private List<PaymentMethod> _paymentMethods;

    /// <summary>
    /// 只读的支付方式集合，外部不能修改集合内容。
    /// </summary>
    public IEnumerable<PaymentMethod> PaymentMethods => _paymentMethods.AsReadOnly();

    /// <summary>
    /// 保护的构造函数，初始化支付方式列表。
    /// </summary>
    protected Buyer()
    {
        _paymentMethods = new List<PaymentMethod>();
    }

    /// <summary>
    /// Buyer 类的公共构造函数，通过身份和姓名初始化买家实例。
    /// 若 identity 或 name 为 null 或只包含空白字符，则抛出异常。
    /// </summary>
    /// <param name="identity">买家的身份标识</param>
    /// <param name="name">买家名称</param>
    public Buyer(string identity, string name) : this()
    {
        // 验证 identity 和 name 参数是否为空
        IdentityGuid = !string.IsNullOrWhiteSpace(identity)
            ? identity
            : throw new ArgumentNullException(nameof(identity));
        Name = !string.IsNullOrWhiteSpace(name)
            ? name
            : throw new ArgumentNullException(nameof(name));
    }

    /// <summary>
    /// 验证是否已存在相同支付方式，如果存在则返回；如果不存在则创建添加新的支付方式。
    /// 同时触发一个领域事件，通知支付方式验证或添加的操作。
    /// </summary>
    /// <param name="cardTypeId">卡类型编号</param>
    /// <param name="alias">支付方式别名</param>
    /// <param name="cardNumber">卡号</param>
    /// <param name="securityNumber">安全码</param>
    /// <param name="cardHolderName">持卡人姓名</param>
    /// <param name="expiration">卡片到期日期</param>
    /// <param name="orderId">订单编号</param>
    /// <returns>现有或新创建的 PaymentMethod 对象</returns>
    public PaymentMethod VerifyOrAddPaymentMethod(
        int cardTypeId, string alias, string cardNumber,
        string securityNumber, string cardHolderName, DateTime expiration, int orderId)
    {
        // 查询已存在的支付方式：卡类型、卡号与到期日匹配认为是同一支付方式
        var existingPayment = _paymentMethods
            .SingleOrDefault(p => p.IsEqualTo(cardTypeId, cardNumber, expiration));

        if (existingPayment != null)
        {
            // 如果存在，触发领域事件，并返回现有支付方式
            AddDomainEvent(new BuyerAndPaymentMethodVerifiedDomainEvent(this, existingPayment, orderId));
            return existingPayment;
        }

        // 如果不存在，创建新的支付方式实例
        var payment = new PaymentMethod(cardTypeId, alias, cardNumber, securityNumber, cardHolderName, expiration);

        // 将新支付方式添加到支付方式集合中
        _paymentMethods.Add(payment);

        // 触发领域事件，通知新支付方式已添加
        AddDomainEvent(new BuyerAndPaymentMethodVerifiedDomainEvent(this, payment, orderId));

        // 返回新创建的支付方式
        return payment;
    }
}
