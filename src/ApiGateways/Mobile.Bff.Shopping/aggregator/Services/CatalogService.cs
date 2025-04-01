namespace Microsoft.eShopOnContainers.Mobile.Shopping.HttpAggregator.Services;

// CatalogService 类实现 ICatalogService 接口，用于调用远程 Catalog 服务获取商品数据
public class CatalogService : ICatalogService
{
    // 定义一个私有只读字段，用于存储 Catalog 客户端实例，通过依赖注入传入
    private readonly Catalog.CatalogClient _client;

    // 构造函数注入 Catalog.CatalogClient 实例
    public CatalogService(Catalog.CatalogClient client)
    {
        _client = client;
    }

    // 根据商品 ID 获取单个商品详情
    public async Task<CatalogItem> GetCatalogItemAsync(int id)
    {
        // 创建商品请求对象并初始化其 Id 属性
        var request = new CatalogItemRequest { Id = id };
        // 异步调用远程服务获取商品详情
        var response = await _client.GetItemByIdAsync(request);
        // 将服务响应映射到 CatalogItem 对象并返回
        return MapToCatalogItemResponse(response);
    }

    // 根据多个商品 ID 获取商品列表
    public async Task<IEnumerable<CatalogItem>> GetCatalogItemsAsync(IEnumerable<int> ids)
    {
        // 构造请求字符串：将 ids 集合转换为逗号分隔的字符串，设置分页参数
        var request = new CatalogItemsRequest { Ids = string.Join(",", ids), PageIndex = 1, PageSize = 10 };
        // 异步调用远程服务获取多个商品详情
        var response = await _client.GetItemsByIdsAsync(request);
        // 使用 LINQ 进行投影映射，将每个响应项转换为 CatalogItem 对象
        return response.Data.Select(MapToCatalogItemResponse);
    }

    // 辅助方法，将 CatalogItemResponse 对象转换为 CatalogItem 对象
    private CatalogItem MapToCatalogItemResponse(CatalogItemResponse catalogItemResponse)
    {
        // 创建并返回一个新的 CatalogItem 对象，并映射响应中的各项属性
        return new CatalogItem
        {
            Id = catalogItemResponse.Id,
            Name = catalogItemResponse.Name,
            PictureUri = catalogItemResponse.PictureUri,
            Price = (decimal)catalogItemResponse.Price
        };
    }
}
