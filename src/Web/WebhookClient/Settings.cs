namespace WebhookClient;

/// <summary>
/// Webhook 客户端配置类，用于存储 Webhook 相关的配置项
/// </summary>
public class Settings
{
    /// <summary>
    /// 身份验证令牌，用于请求验证
    /// </summary>
    public string Token { get; set; }

    /// <summary>
    /// 身份验证服务的 URL 地址
    /// </summary>
    public string IdentityUrl { get; set; }

    /// <summary>
    /// 回调 URL 地址，用于接收 Webhook 回调请求
    /// </summary>
    public string CallBackUrl { get; set; }

    /// <summary>
    /// Webhook 服务的 URL 地址，用于发送 Webhook 请求
    /// </summary>
    public string WebhooksUrl { get; set; }

    /// <summary>
    /// 当前应用程序的 URL 地址
    /// </summary>
    public string SelfUrl { get; set; }

    /// <summary>
    /// 是否验证令牌，用于控制是否进行令牌验证
    /// </summary>
    public bool ValidateToken { get; set; }
}
