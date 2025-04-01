namespace Microsoft.eShopOnContainers.Web.Shopping.HttpAggregator.Services;

// OrderApiClient 类用于调用订单服务接口，根据购物篮数据获取订单草稿
public class OrderApiClient : IOrderApiClient
{
    // 用于发送 HTTP 请求
    private readonly HttpClient _apiClient;

    // 日志记录器，用于记录运行信息及错误
    private readonly ILogger<OrderApiClient> _logger;

    // 存储各后端服务的 URL 配置
    private readonly UrlsConfig _urls;

    // 构造函数，通过依赖注入初始化 HttpClient、ILogger 以及 URL 配置
    public OrderApiClient(HttpClient httpClient, ILogger<OrderApiClient> logger, IOptions<UrlsConfig> config)
    {
        _apiClient = httpClient;
        _logger = logger;
        _urls = config.Value;
    }

    // 根据购物篮数据调用订单服务，返回订单草稿
    public async Task<OrderData> GetOrderDraftFromBasketAsync(BasketData basket)
    {
        // 拼接请求的 URL：订单服务基础地址 + 获取订单草稿操作对应的 URL 路径
        var url = $"{_urls.Orders}{UrlsConfig.OrdersOperations.GetOrderDraft()}";

        // 将购物篮数据序列化成 JSON，并创建请求内容，指定 UTF8 编码和 application/json 媒体类型
        var content = new StringContent(JsonSerializer.Serialize(basket), System.Text.Encoding.UTF8, "application/json");

        // 发送 POST 请求调用订单服务
        var response = await _apiClient.PostAsync(url, content);

        // 如果响应状态码不是成功，则抛出异常
        response.EnsureSuccessStatusCode();

        // 读取响应内容（JSON 字符串）
        var ordersDraftResponse = await response.Content.ReadAsStringAsync();

        // 反序列化 JSON 字符串为 OrderData 对象，并开启忽略大小写属性匹配
        return JsonSerializer.Deserialize<OrderData>(ordersDraftResponse, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }
}
