namespace Microsoft.eShopOnContainers.WebMVC.Services;

using Microsoft.eShopOnContainers.WebMVC.ViewModels;

/// <summary>
/// 购物篮服务实现类，负责与购物篮微服务API进行通信
/// </summary>
public class BasketService : IBasketService
{
    private readonly IOptions<AppSettings> _settings;  // 应用程序配置
    private readonly HttpClient _apiClient;            // HTTP客户端，用于API调用
    private readonly ILogger<BasketService> _logger;   // 日志记录器
    private readonly string _basketByPassUrl;          // 购物篮旁路URL
    private readonly string _purchaseUrl;              // 购买URL

    /// <summary>
    /// 构造函数，初始化服务所需依赖
    /// </summary>
    /// <param name="httpClient">HTTP客户端</param>
    /// <param name="settings">应用配置</param>
    /// <param name="logger">日志记录器</param>
    public BasketService(HttpClient httpClient, IOptions<AppSettings> settings, ILogger<BasketService> logger)
    {
        _apiClient = httpClient;
        _settings = settings;
        _logger = logger;

        // 初始化API路径
        _basketByPassUrl = $"{_settings.Value.PurchaseUrl}/b/api/v1/basket";
        _purchaseUrl = $"{_settings.Value.PurchaseUrl}/api/v1";
    }

    /// <summary>
    /// 获取指定用户的购物篮数据
    /// </summary>
    /// <param name="user">应用程序用户</param>
    /// <returns>购物篮对象</returns>
    public async Task<Basket> GetBasket(ApplicationUser user)
    {
        var uri = API.Basket.GetBasket(_basketByPassUrl, user.Id);
        _logger.LogDebug("[GetBasket] -> Calling {Uri} to get the basket", uri);
        var response = await _apiClient.GetAsync(uri);
        _logger.LogDebug("[GetBasket] -> response code {StatusCode}", response.StatusCode);
        var responseString = await response.Content.ReadAsStringAsync();

        // 如果响应为空，则创建新的购物篮；否则反序列化返回的购物篮数据
        return string.IsNullOrEmpty(responseString) ?
            new Basket() { BuyerId = user.Id } :
            JsonSerializer.Deserialize<Basket>(responseString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
    }

    /// <summary>
    /// 更新购物篮数据
    /// </summary>
    /// <param name="basket">要更新的购物篮对象</param>
    /// <returns>更新后的购物篮</returns>
    public async Task<Basket> UpdateBasket(Basket basket)
    {
        var uri = API.Basket.UpdateBasket(_basketByPassUrl);

        // 将购物篮对象序列化为JSON并准备HTTP请求内容
        var basketContent = new StringContent(JsonSerializer.Serialize(basket), System.Text.Encoding.UTF8, "application/json");

        var response = await _apiClient.PostAsync(uri, basketContent);

        // 确保HTTP请求成功
        response.EnsureSuccessStatusCode();

        return basket;
    }

    /// <summary>
    /// 结算购物篮，完成下单过程
    /// </summary>
    /// <param name="basket">购物篮数据传输对象</param>
    public async Task Checkout(BasketDTO basket)
    {
        var uri = API.Basket.CheckoutBasket(_basketByPassUrl);
        var basketContent = new StringContent(JsonSerializer.Serialize(basket), System.Text.Encoding.UTF8, "application/json");

        _logger.LogInformation("Uri checkout {uri}", uri);

        var response = await _apiClient.PostAsync(uri, basketContent);

        // 确保结算请求成功
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// 设置购物篮中商品的数量
    /// </summary>
    /// <param name="user">应用程序用户</param>
    /// <param name="quantities">商品ID和对应数量的字典</param>
    /// <returns>更新后的购物篮</returns>
    public async Task<Basket> SetQuantities(ApplicationUser user, Dictionary<string, int> quantities)
    {
        var uri = API.Purchase.UpdateBasketItem(_purchaseUrl);

        // 构建购物篮更新请求对象
        var basketUpdate = new
        {
            BasketId = user.Id,
            Updates = quantities.Select(kvp => new
            {
                BasketItemId = kvp.Key,
                NewQty = kvp.Value
            }).ToArray()
        };

        var basketContent = new StringContent(JsonSerializer.Serialize(basketUpdate), System.Text.Encoding.UTF8, "application/json");

        var response = await _apiClient.PutAsync(uri, basketContent);

        // 确保HTTP请求成功
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();

        // 反序列化并返回更新后的购物篮
        return JsonSerializer.Deserialize<Basket>(jsonResponse, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    /// <summary>
    /// 获取订单草稿，用于结算前预览
    /// </summary>
    /// <param name="basketId">购物篮ID</param>
    /// <returns>订单对象</returns>
    public async Task<Order> GetOrderDraft(string basketId)
    {
        var uri = API.Purchase.GetOrderDraft(_purchaseUrl, basketId);

        var responseString = await _apiClient.GetStringAsync(uri);

        // 反序列化订单响应
        var response = JsonSerializer.Deserialize<Order>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return response;
    }

    /// <summary>
    /// 向购物篮添加商品
    /// </summary>
    /// <param name="user">应用程序用户</param>
    /// <param name="productId">要添加的商品ID</param>
    public async Task AddItemToBasket(ApplicationUser user, int productId)
    {
        var uri = API.Purchase.AddItemToBasket(_purchaseUrl);

        // 构建添加商品的请求对象
        var newItem = new
        {
            CatalogItemId = productId,
            BasketId = user.Id,
            Quantity = 1  // 默认添加1个
        };

        var basketContent = new StringContent(JsonSerializer.Serialize(newItem), System.Text.Encoding.UTF8, "application/json");

        // 发送添加商品请求
        var response = await _apiClient.PostAsync(uri, basketContent);
    }
}
