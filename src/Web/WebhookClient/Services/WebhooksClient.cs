namespace WebhookClient.Services;

/// <summary>
/// Webhook客户端服务，用于与Webhook API进行通信
/// </summary>
public class WebhooksClient : IWebhooksClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly Settings _settings;

    /// <summary>
    /// 初始化WebhooksClient的新实例
    /// </summary>
    /// <param name="httpClientFactory">HTTP客户端工厂，用于创建HTTP客户端</param>
    /// <param name="settings">应用程序配置选项</param>
    public WebhooksClient(IHttpClientFactory httpClientFactory, IOptions<Settings> settings)
    {
        _httpClientFactory = httpClientFactory;
        _settings = settings.Value;
    }

    /// <summary>
    /// 从API加载所有webhook订阅
    /// </summary>
    /// <returns>webhook响应集合</returns>
    public async Task<IEnumerable<WebhookResponse>> LoadWebhooks()
    {
        // 创建具有授权策略的HTTP客户端
        var client = _httpClientFactory.CreateClient("GrantClient");

        // 发送GET请求到Webhook API
        var response = await client.GetAsync(_settings.WebhooksUrl + "/api/v1/webhooks");

        // 读取响应内容
        var json = await response.Content.ReadAsStringAsync();

        // 将JSON响应反序列化为WebhookResponse对象集合
        var subscriptions = JsonSerializer.Deserialize<IEnumerable<WebhookResponse>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true // 忽略属性名称大小写
        });

        return subscriptions;
    }
}
