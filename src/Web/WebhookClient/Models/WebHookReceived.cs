namespace WebhookClient.Models;

/// <summary>
/// 表示从外部服务接收到的 webhook 数据的模型
/// </summary>
public class WebHookReceived
{
    /// <summary>
    /// 获取或设置 webhook 接收的时间
    /// </summary>
    public DateTime When { get; set; }

    /// <summary>
    /// 获取或设置 webhook 携带的数据内容
    /// </summary>
    public string Data { get; set; }

    /// <summary>
    /// 获取或设置用于验证 webhook 请求的令牌
    /// </summary>
    public string Token { get; set; }
}
