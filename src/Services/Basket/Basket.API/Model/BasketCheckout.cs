namespace Basket.API.Model;

/// <summary>
/// 代表购物篮结账信息的模型（用于结账时传递相关数据）
/// </summary>
public class BasketCheckout
{
    /// <summary>
    /// 城市
    /// </summary>
    public string City { get; set; }

    /// <summary>
    /// 街道地址
    /// </summary>
    public string Street { get; set; }

    /// <summary>
    /// 州/省份
    /// </summary>
    public string State { get; set; }

    /// <summary>
    /// 国家
    /// </summary>
    public string Country { get; set; }

    /// <summary>
    /// 邮编
    /// </summary>
    public string ZipCode { get; set; }

    /// <summary>
    /// 信用卡号码
    /// </summary>
    public string CardNumber { get; set; }

    /// <summary>
    /// 持卡人姓名
    /// </summary>
    public string CardHolderName { get; set; }

    /// <summary>
    /// 信用卡过期时间
    /// </summary>
    public DateTime CardExpiration { get; set; }

    /// <summary>
    /// 信用卡安全码
    /// </summary>
    public string CardSecurityNumber { get; set; }

    /// <summary>
    /// 信用卡类型的标识符
    /// </summary>
    public int CardTypeId { get; set; }

    /// <summary>
    /// 购买者信息
    /// </summary>
    public string Buyer { get; set; }

    /// <summary>
    /// 请求标识符，用于跟踪和幂等性控制
    /// </summary>
    public Guid RequestId { get; set; }
}
