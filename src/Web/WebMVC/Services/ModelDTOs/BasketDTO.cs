namespace WebMVC.Services.ModelDTOs;

/// <summary>
/// 购物篮数据传输对象，用于封装结账过程中的地址和支付信息
/// </summary>
public record BasketDTO
{
    /// <summary>
    /// 城市
    /// </summary>
    [Required]
    public string City { get; init; }

    /// <summary>
    /// 街道地址
    /// </summary>
    [Required]
    public string Street { get; init; }

    /// <summary>
    /// 州/省
    /// </summary>
    [Required]
    public string State { get; init; }

    /// <summary>
    /// 国家
    /// </summary>
    [Required]
    public string Country { get; init; }

    /// <summary>
    /// 邮政编码
    /// </summary>
    public string ZipCode { get; init; }

    /// <summary>
    /// 支付卡号
    /// </summary>
    [Required]
    public string CardNumber { get; init; }

    /// <summary>
    /// 持卡人姓名
    /// </summary>
    [Required]
    public string CardHolderName { get; init; }

    /// <summary>
    /// 卡片到期日期
    /// </summary>
    [Required]
    public DateTime CardExpiration { get; init; }

    /// <summary>
    /// 卡片安全码
    /// </summary>
    [Required]
    public string CardSecurityNumber { get; init; }

    /// <summary>
    /// 卡片类型ID
    /// </summary>
    public int CardTypeId { get; init; }

    /// <summary>
    /// 买家信息
    /// </summary>
    public string Buyer { get; init; }

    /// <summary>
    /// 请求ID，用于跟踪请求
    /// </summary>
    [Required]
    public Guid RequestId { get; init; }
}
