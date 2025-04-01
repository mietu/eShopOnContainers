namespace WebhookClient.Models;

/// <summary>
/// 表示从 Webhook 接收到的数据
/// </summary>
public class WebhookData
{
    /// <summary>
    /// 获取或设置 Webhook 事件发生的时间
    /// </summary>
    public DateTime When { get; set; }

    /// <summary>
    /// 获取或设置 Webhook 的有效负载内容
    /// </summary>
    public string Payload { get; set; }

    /// <summary>
    /// 获取或设置 Webhook 的事件类型
    /// </summary>
    public string Type { get; set; }
}
