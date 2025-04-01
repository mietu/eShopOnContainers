namespace Microsoft.eShopOnContainers.Web.Shopping.HttpAggregator.Services;

// CatalogService 实现了 ICatalogService 接口，用于通过 gRPC 客户端调用 Catalog 服务
public class CatalogService : ICatalogService
{
    // gRPC 客户端，用于调用 Catalog 服务的 API
    private readonly Catalog.CatalogClient _client;
    // 日志记录器，用于记录请求和响应的信息
    private readonly ILogger<CatalogService> _logger;

    // 构造函数，注入 gRPC 客户端和日志记录器
    public CatalogService(Catalog.CatalogClient client, ILogger<CatalogService> logger)
    {
        _client = client;
        _logger = logger;
    }

    // 根据商品 id 异步获取单个商品详情
    public async Task<CatalogItem> GetCatalogItemAsync(int id)
    {
        // 创建请求对象，传入商品 id
        var request = new CatalogItemRequest { Id = id };
        // 记录请求日志
        _logger.LogInformation("grpc request {@request}", request);
        // 调用 gRPC 服务方法获取响应
        var response = await _client.GetItemByIdAsync(request);
        // 记录响应日志
        _logger.LogInformation("grpc response {@response}", response);
        // 将 gRPC 响应映射到内部 CatalogItem 实体并返回
        return MapToCatalogItemResponse(response);
    }

    // 根据多个商品 id 异步获取多个商品详情
    public async Task<IEnumerable<CatalogItem>> GetCatalogItemsAsync(IEnumerable<int> ids)
    {
        // 将多个 id 用逗号分隔生成字符串，并创建请求对象，同时设置分页参数
        var request = new CatalogItemsRequest { Ids = string.Join(",", ids), PageIndex = 1, PageSize = 10 };
        // 记录请求日志
        _logger.LogInformation("grpc request {@request}", request);
        // 调用 gRPC 服务方法获取响应
        var response = await _client.GetItemsByIdsAsync(request);
        // 记录响应日志
        _logger.LogInformation("grpc response {@response}", response);
        // 将响应中的每个 CatalogItemResponse 映射为内部 CatalogItem 实体
        return response.Data.Select(this.MapToCatalogItemResponse);
    }

    // 将 gRPC 返回的 CatalogItemResponse 对象转换为内部使用的 CatalogItem 对象
    private CatalogItem MapToCatalogItemResponse(CatalogItemResponse catalogItemResponse)
    {
        return new CatalogItem
        {
            Id = catalogItemResponse.Id,
            Name = catalogItemResponse.Name,
            PictureUri = catalogItemResponse.PictureUri,
            Price = (decimal)catalogItemResponse.Price
        };
    }
}
