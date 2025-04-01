namespace Microsoft.eShopOnContainers.WebMVC.ViewModels;

/// <summary>
/// 应用程序用户类，扩展了Identity用户基类，用于存储额外的用户信息
/// </summary>
public class ApplicationUser : IdentityUser
{
    // 支付信息
    /// <summary>
    /// 信用卡卡号
    /// </summary>
    public string CardNumber { get; set; }

    /// <summary>
    /// 信用卡安全码
    /// </summary>
    public string SecurityNumber { get; set; }

    /// <summary>
    /// 信用卡到期日期
    /// </summary>
    public string Expiration { get; set; }

    /// <summary>
    /// 持卡人姓名
    /// </summary>
    public string CardHolderName { get; set; }

    /// <summary>
    /// 卡类型（如Visa=1, MasterCard=2等）
    /// </summary>
    public int CardType { get; set; }

    // 地址信息
    /// <summary>
    /// 街道地址
    /// </summary>
    public string Street { get; set; }

    /// <summary>
    /// 城市
    /// </summary>
    public string City { get; set; }

    /// <summary>
    /// 州/省
    /// </summary>
    public string State { get; set; }

    /// <summary>
    /// 州/省代码
    /// </summary>
    public string StateCode { get; set; }

    /// <summary>
    /// 国家
    /// </summary>
    public string Country { get; set; }

    /// <summary>
    /// 国家代码
    /// </summary>
    public string CountryCode { get; set; }

    /// <summary>
    /// 邮政编码
    /// </summary>
    public string ZipCode { get; set; }

    /// <summary>
    /// 地址纬度
    /// </summary>
    public double Latitude { get; set; }

    /// <summary>
    /// 地址经度
    /// </summary>
    public double Longitude { get; set; }

    // 个人信息
    /// <summary>
    /// 用户名字（必填）
    /// </summary>
    [Required]
    public string Name { get; set; }

    /// <summary>
    /// 用户姓氏（必填）
    /// </summary>
    [Required]
    public string LastName { get; set; }
}
