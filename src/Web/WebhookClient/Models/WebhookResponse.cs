namespace WebhookClient.Models;

/// <summary>
/// 表示Webhook响应的数据模型
/// </summary>
public class WebhookResponse
{
    /// <summary>
    /// 获取或设置响应的日期时间
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// 获取或设置目标URL (Destination URL)
    /// </summary>
    public string DestUrl { get; set; }

    /// <summary>
    /// 获取或设置与webhook关联的令牌
    /// </summary>
    public string Token { get; set; }
}
