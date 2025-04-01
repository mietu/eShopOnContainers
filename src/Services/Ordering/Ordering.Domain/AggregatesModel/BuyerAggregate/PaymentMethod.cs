namespace Microsoft.eShopOnContainers.Services.Ordering.Domain.AggregatesModel.BuyerAggregate;

/// <summary>
/// PaymentMethod 表示与买家关联的支付方式。
/// </summary>
public class PaymentMethod : Entity
{
    // 支付卡的友好名称（别名）
    private string _alias;

    // 支付卡号
    private string _cardNumber;

    // 支付卡的安全码
    private string _securityNumber;

    // 持卡人姓名
    private string _cardHolderName;

    // 支付卡的过期日期
    private DateTime _expiration;

    // 卡类型标识符（例如 Visa, Amex 等）
    private int _cardTypeId;

    // 与支付卡相关的卡类型信息
    public CardType CardType { get; private set; }

    // 用于 ORM 或序列化的无参构造函数
    protected PaymentMethod() { }

    /// <summary>
    /// 使用完整支付卡信息初始化 PaymentMethod 实例。
    /// </summary>
    /// <param name="cardTypeId">卡类型标识符</param>
    /// <param name="alias">支付卡别名</param>
    /// <param name="cardNumber">支付卡号</param>
    /// <param name="securityNumber">安全码</param>
    /// <param name="cardHolderName">持卡人姓名</param>
    /// <param name="expiration">卡的过期日期</param>
    public PaymentMethod(int cardTypeId, string alias, string cardNumber, string securityNumber, string cardHolderName, DateTime expiration)
    {
        // 验证卡号不为空，否则抛出异常
        _cardNumber = !string.IsNullOrWhiteSpace(cardNumber)
            ? cardNumber
            : throw new OrderingDomainException(nameof(cardNumber));

        // 验证安全码不为空，否则抛出异常
        _securityNumber = !string.IsNullOrWhiteSpace(securityNumber)
            ? securityNumber
            : throw new OrderingDomainException(nameof(securityNumber));

        // 验证持卡人姓名不为空，否则抛出异常
        _cardHolderName = !string.IsNullOrWhiteSpace(cardHolderName)
            ? cardHolderName
            : throw new OrderingDomainException(nameof(cardHolderName));

        // 检查过期日期是否早于当前时间，如果是则抛出异常
        if (expiration < DateTime.UtcNow)
        {
            throw new OrderingDomainException(nameof(expiration));
        }

        // 设置别名、到期日期以及卡类型标识符
        _alias = alias;
        _expiration = expiration;
        _cardTypeId = cardTypeId;
    }

    /// <summary>
    /// 判断当前支付方式是否与提供的卡类型、卡号及到期日期一致。
    /// </summary>
    /// <param name="cardTypeId">卡类型标识符</param>
    /// <param name="cardNumber">支付卡号</param>
    /// <param name="expiration">卡的过期日期</param>
    /// <returns>若匹配返回 true，否则返回 false</returns>
    public bool IsEqualTo(int cardTypeId, string cardNumber, DateTime expiration)
    {
        return _cardTypeId == cardTypeId
            && _cardNumber == cardNumber
            && _expiration == expiration;
    }
}
