namespace WebMVC.Controllers;

/// <summary>
/// 测试API请求的负载数据模型
/// </summary>
class TestPayload
{
    /// <summary>
    /// 商品目录项ID
    /// </summary>
    public int CatalogItemId { get; set; }

    /// <summary>
    /// 购物篮ID
    /// </summary>
    public string BasketId { get; set; }

    /// <summary>
    /// 商品数量
    /// </summary>
    public int Quantity { get; set; }
}

/// <summary>
/// 测试控制器，用于验证API网关和微服务通信
/// 需要用户授权才能访问
/// </summary>
[Authorize]
public class TestController : Controller
{
    private readonly IHttpClientFactory _client;        // HTTP客户端工厂，用于创建HTTP请求客户端
    private readonly IIdentityParser<ApplicationUser> _appUserParser;  // 用户身份解析器，解析当前用户信息

    /// <summary>
    /// 构造函数，通过依赖注入获取所需服务
    /// </summary>
    /// <param name="client">HTTP客户端工厂</param>
    /// <param name="identityParser">用户身份解析器</param>
    public TestController(IHttpClientFactory client, IIdentityParser<ApplicationUser> identityParser)
    {
        _client = client;
        _appUserParser = identityParser;
    }

    /// <summary>
    /// 测试通过Ocelot API网关向购物篮微服务发送请求
    /// </summary>
    /// <returns>请求结果</returns>
    public async Task<IActionResult> Ocelot()
    {
        // API网关地址，指向购物篮微服务的添加商品API
        var url = "http://apigw/shopping/api/v1/basket/items";

        // 创建请求负载，添加商品到购物篮
        var payload = new TestPayload()
        {
            CatalogItemId = 1,           // 测试商品ID
            Quantity = 1,                // 添加数量为1
            BasketId = _appUserParser.Parse(User).Id  // 从当前用户获取购物篮ID
        };

        // 将负载序列化为JSON并创建StringContent
        var content = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");

        // 使用命名的HTTP客户端发送POST请求
        var response = await _client.CreateClient(nameof(IBasketService))
            .PostAsync(url, content);

        // 处理响应结果
        if (response.IsSuccessStatusCode)
        {
            // 请求成功，返回响应内容
            var str = await response.Content.ReadAsStringAsync();
            return Ok(str);
        }
        else
        {
            // 请求失败，返回状态码和原因
            return Ok(new { response.StatusCode, response.ReasonPhrase });
        }
    }
}
