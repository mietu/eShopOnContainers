namespace Microsoft.eShopOnContainers.Mobile.Shopping.HttpAggregator.Services;

/// <summary>
/// OrderApiClient 类用于调用订单服务接口，根据购物篮数据获取订单草稿
/// </summary>
public class OrderApiClient : IOrderApiClient
{
    // 用于发送 HTTP 请求到订单服务
    private readonly HttpClient _apiClient;
    // 日志记录器，用于记录运行信息及错误
    private readonly ILogger<OrderApiClient> _logger;
    // 存储系统中所有的 URL 配置
    private readonly UrlsConfig _urls;

    /// <summary>
    /// 构造函数，通过依赖注入初始化 HttpClient、ILogger 以及 URL 配置
    /// </summary>
    /// <param name="httpClient">用于发送 HTTP 请求的 HttpClient</param>
    /// <param name="logger">日志记录器</param>
    /// <param name="config">包含 URL 配置的 IOptions 包装对象</param>
    public OrderApiClient(HttpClient httpClient, ILogger<OrderApiClient> logger, IOptions<UrlsConfig> config)
    {
        _apiClient = httpClient;
        _logger = logger;
        _urls = config.Value;
    }

    /// <summary>
    /// 根据购物篮数据调用订单服务接口，获取订单草稿
    /// </summary>
    /// <param name="basket">购物篮数据，包含买家和购物项信息</param>
    /// <returns>返回订单草稿数据</returns>
    public async Task<OrderData> GetOrderDraftFromBasketAsync(BasketData basket)
    {
        // 构造调用订单服务的 URI，将基本 URL 与获取订单草稿操作的 URL 拼接
        var uri = _urls.Orders + UrlsConfig.OrdersOperations.GetOrderDraft();

        // 将购物篮数据序列化成 JSON，并构造 HTTP 请求正文
        var content = new StringContent(JsonSerializer.Serialize(basket), System.Text.Encoding.UTF8, "application/json");

        // 发送 POST 请求调用订单服务
        var response = await _apiClient.PostAsync(uri, content);

        // 如果响应状态码不是成功状态码，则抛出异常
        response.EnsureSuccessStatusCode();

        // 读取响应的内容字符串
        var ordersDraftResponse = await response.Content.ReadAsStringAsync();

        // 反序列化响应字符串成 OrderData 对象，并设置忽略大小写选项
        return JsonSerializer.Deserialize<OrderData>(ordersDraftResponse, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }
}
