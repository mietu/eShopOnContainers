namespace Microsoft.eShopOnContainers.WebMVC.Services;

/// <summary>
/// 目录服务实现类，负责与目录 API 通信获取产品数据
/// </summary>
public class CatalogService : ICatalogService
{
    private readonly IOptions<AppSettings> _settings; // 应用程序配置
    private readonly HttpClient _httpClient; // HTTP 客户端用于发送请求
    private readonly ILogger<CatalogService> _logger; // 日志记录器

    private readonly string _remoteServiceBaseUrl; // 远程服务基础 URL

    /// <summary>
    /// 构造函数，通过依赖注入初始化服务
    /// </summary>
    /// <param name="httpClient">HTTP 客户端实例</param>
    /// <param name="logger">日志记录器</param>
    /// <param name="settings">应用程序配置</param>
    public CatalogService(HttpClient httpClient, ILogger<CatalogService> logger, IOptions<AppSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings;
        _logger = logger;

        // 构建目录 API 的基础 URL
        _remoteServiceBaseUrl = $"{_settings.Value.PurchaseUrl}/c/api/v1/catalog/";
    }

    /// <summary>
    /// 获取目录商品列表，支持分页和筛选
    /// </summary>
    /// <param name="page">页码</param>
    /// <param name="take">每页数量</param>
    /// <param name="brand">品牌 ID 筛选</param>
    /// <param name="type">类型 ID 筛选</param>
    /// <returns>包含商品列表的目录对象</returns>
    public async Task<Catalog> GetCatalogItems(int page, int take, int? brand, int? type)
    {
        // 构建 API URL
        var uri = API.Catalog.GetAllCatalogItems(_remoteServiceBaseUrl, page, take, brand, type);

        // 发送 GET 请求获取数据
        var responseString = await _httpClient.GetStringAsync(uri);

        // 将 JSON 响应反序列化为 Catalog 对象
        var catalog = JsonSerializer.Deserialize<Catalog>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true // 属性名称不区分大小写
        });

        return catalog;
    }

    /// <summary>
    /// 获取所有品牌列表，用于 UI 下拉选择
    /// </summary>
    /// <returns>品牌选择列表项集合</returns>
    public async Task<IEnumerable<SelectListItem>> GetBrands()
    {
        // 构建 API URL
        var uri = API.Catalog.GetAllBrands(_remoteServiceBaseUrl);

        // 发送 GET 请求获取数据
        var responseString = await _httpClient.GetStringAsync(uri);

        var items = new List<SelectListItem>();

        // 添加"全部"选项
        items.Add(new SelectListItem() { Value = null, Text = "All", Selected = true });

        // 解析 JSON 数据
        using var brands = JsonDocument.Parse(responseString);

        // 遍历品牌数组，构建选择列表项
        foreach (JsonElement brand in brands.RootElement.EnumerateArray())
        {
            items.Add(new SelectListItem()
            {
                Value = brand.GetProperty("id").ToString(),
                Text = brand.GetProperty("brand").ToString()
            });
        }

        return items;
    }

    /// <summary>
    /// 获取所有商品类型列表，用于 UI 下拉选择
    /// </summary>
    /// <returns>类型选择列表项集合</returns>
    public async Task<IEnumerable<SelectListItem>> GetTypes()
    {
        // 构建 API URL
        var uri = API.Catalog.GetAllTypes(_remoteServiceBaseUrl);

        // 发送 GET 请求获取数据
        var responseString = await _httpClient.GetStringAsync(uri);

        var items = new List<SelectListItem>();

        // 添加"全部"选项
        items.Add(new SelectListItem() { Value = null, Text = "All", Selected = true });

        // 解析 JSON 数据
        using var catalogTypes = JsonDocument.Parse(responseString);

        // 遍历类型数组，构建选择列表项
        foreach (JsonElement catalogType in catalogTypes.RootElement.EnumerateArray())
        {
            items.Add(new SelectListItem()
            {
                Value = catalogType.GetProperty("id").ToString(),
                Text = catalogType.GetProperty("type").ToString()
            });
        }

        return items;
    }
}
