namespace WebhookClient.Models;

/// <summary>
/// 表示 Webhook 订阅请求的数据模型
/// </summary>
public class WebhookSubscriptionRequest
{
    /// <summary>
    /// 获取或设置 Webhook 的回调 URL
    /// 当事件发生时，系统将向此 URL 发送通知
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// 获取或设置用于验证 Webhook 请求的安全令牌
    /// 通常用于确保只有授权方才能接收 Webhook 通知
    /// </summary>
    public string Token { get; set; }

    /// <summary>
    /// 获取或设置要订阅的事件类型
    /// 指定想要接收哪种类型的事件通知
    /// </summary>
    public string Event { get; set; }

    /// <summary>
    /// 获取或设置授权 URL
    /// 通常用于 OAuth 流程或类似授权机制
    /// </summary>
    public string GrantUrl { get; set; }
}
