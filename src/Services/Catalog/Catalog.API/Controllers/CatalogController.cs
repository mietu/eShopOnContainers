namespace Microsoft.eShopOnContainers.Services.Catalog.API.Controllers;

// 使用 ApiController 特性标识此控制器为 Web API 控制器，并指定路由前缀
[Route("api/v1/[controller]")]
[ApiController]
public class CatalogController : ControllerBase
{
    // 数据上下文、配置和集成事件服务的依赖
    private readonly CatalogContext _catalogContext;
    private readonly CatalogSettings _settings;
    private readonly ICatalogIntegrationEventService _catalogIntegrationEventService;

    // 构造函数，通过依赖注入初始化所需的服务和设置
    public CatalogController(CatalogContext context, IOptionsSnapshot<CatalogSettings> settings, ICatalogIntegrationEventService catalogIntegrationEventService)
    {
        _catalogContext = context ?? throw new ArgumentNullException(nameof(context));
        _catalogIntegrationEventService = catalogIntegrationEventService ?? throw new ArgumentNullException(nameof(catalogIntegrationEventService));
        _settings = settings.Value;

        // 关闭查询跟踪行为，提高查询效率
        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    /// <summary>
    /// GET api/v1/catalog/items[?pageSize=3&pageIndex=10]
    /// 获取商品分页列表或根据 ids 获取指定的商品
    /// </summary>
    [HttpGet]
    [Route("items")]
    [ProducesResponseType(typeof(PaginatedItemsViewModel<CatalogItem>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(IEnumerable<CatalogItem>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> ItemsAsync([FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 0, string ids = null)
    {
        // 如果提供了 ids 参数，则只返回指定的商品
        if (!string.IsNullOrEmpty(ids))
        {
            var items = await GetItemsByIdsAsync(ids);

            if (!items.Any())
            {
                return BadRequest("ids value invalid. Must be comma-separated list of numbers");
            }

            return Ok(items);
        }

        // 否则，进行分页查询所有商品
        var totalItems = await _catalogContext.CatalogItems.LongCountAsync();

        var itemsOnPage = await _catalogContext.CatalogItems
            .OrderBy(c => c.Name)
            .Skip(pageSize * pageIndex)
            .Take(pageSize)
            .ToListAsync();

        /* The "awesome" fix for testing Devspaces */
        /*
        foreach (var pr in itemsOnPage) {
            pr.Name = "Awesome " + pr.Name;
        }
        */

        // 修改商品图片 URI
        itemsOnPage = ChangeUriPlaceholder(itemsOnPage);

        var model = new PaginatedItemsViewModel<CatalogItem>(pageIndex, pageSize, totalItems, itemsOnPage);

        return Ok(model);
    }

    /// <summary>
    /// 根据逗号分隔的 id 列表查找相应的商品
    /// </summary>
    private async Task<List<CatalogItem>> GetItemsByIdsAsync(string ids)
    {
        // 将字符串拆分，尝试转换为数字
        var numIds = ids.Split(',').Select(id => (Ok: int.TryParse(id, out int x), Value: x));

        if (!numIds.All(nid => nid.Ok))
        {
            return new List<CatalogItem>();
        }

        // 获取所有有效的 id 值
        var idsToSelect = numIds.Select(id => id.Value);

        var items = await _catalogContext.CatalogItems
            .Where(ci => idsToSelect.Contains(ci.Id))
            .ToListAsync();

        // 调整图片 URI
        items = ChangeUriPlaceholder(items);

        return items;
    }

    /// <summary>
    /// GET api/v1/catalog/items/{id}
    /// 根据 id 获取单个商品详情
    /// </summary>
    [HttpGet]
    [Route("items/{id:int}")]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(CatalogItem), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<CatalogItem>> ItemByIdAsync(int id)
    {
        if (id <= 0)
        {
            return BadRequest();
        }

        // 根据 id 查询单个商品
        var item = await _catalogContext.CatalogItems.SingleOrDefaultAsync(ci => ci.Id == id);

        var baseUri = _settings.PicBaseUrl;
        var azureStorageEnabled = _settings.AzureStorageEnabled;
        // 填充商品图片 URI
        item.FillProductUrl(baseUri, azureStorageEnabled: azureStorageEnabled);

        if (item != null)
        {
            return item;
        }

        return NotFound();
    }

    /// <summary>
    /// GET api/v1/catalog/items/withname/{name}[?pageSize=3&pageIndex=10]
    /// 根据商品名称前缀获取分页列表
    /// </summary>
    [HttpGet]
    [Route("items/withname/{name:minlength(1)}")]
    [ProducesResponseType(typeof(PaginatedItemsViewModel<CatalogItem>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<PaginatedItemsViewModel<CatalogItem>>> ItemsWithNameAsync(string name, [FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 0)
    {
        // 查询名称以指定前缀开头的商品
        var totalItems = await _catalogContext.CatalogItems
            .Where(c => c.Name.StartsWith(name))
            .LongCountAsync();

        var itemsOnPage = await _catalogContext.CatalogItems
            .Where(c => c.Name.StartsWith(name))
            .Skip(pageSize * pageIndex)
            .Take(pageSize)
            .ToListAsync();

        // 修改商品图片 URI
        itemsOnPage = ChangeUriPlaceholder(itemsOnPage);

        return new PaginatedItemsViewModel<CatalogItem>(pageIndex, pageSize, totalItems, itemsOnPage);
    }

    /// <summary>
    /// GET api/v1/catalog/items/type/{catalogTypeId}/brand/{catalogBrandId}
    /// 根据商品类型和品牌获取分页列表（品牌为可选）
    /// </summary>
    [HttpGet]
    [Route("items/type/{catalogTypeId}/brand/{catalogBrandId:int?}")]
    [ProducesResponseType(typeof(PaginatedItemsViewModel<CatalogItem>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<PaginatedItemsViewModel<CatalogItem>>> ItemsByTypeIdAndBrandIdAsync(int catalogTypeId, int? catalogBrandId, [FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 0)
    {
        // 构建查询，首先过滤类型
        var root = (IQueryable<CatalogItem>)_catalogContext.CatalogItems;

        root = root.Where(ci => ci.CatalogTypeId == catalogTypeId);

        // 如果指定了品牌，则进一步过滤
        if (catalogBrandId.HasValue)
        {
            root = root.Where(ci => ci.CatalogBrandId == catalogBrandId);
        }

        var totalItems = await root.LongCountAsync();

        var itemsOnPage = await root
            .Skip(pageSize * pageIndex)
            .Take(pageSize)
            .ToListAsync();

        itemsOnPage = ChangeUriPlaceholder(itemsOnPage);

        return new PaginatedItemsViewModel<CatalogItem>(pageIndex, pageSize, totalItems, itemsOnPage);
    }

    /// <summary>
    /// GET api/v1/catalog/items/type/all/brand/{catalogBrandId}
    /// 根据品牌获取分页商品列表（类型不做限制）
    /// </summary>
    [HttpGet]
    [Route("items/type/all/brand/{catalogBrandId:int?}")]
    [ProducesResponseType(typeof(PaginatedItemsViewModel<CatalogItem>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<PaginatedItemsViewModel<CatalogItem>>> ItemsByBrandIdAsync(int? catalogBrandId, [FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 0)
    {
        var root = (IQueryable<CatalogItem>)_catalogContext.CatalogItems;

        if (catalogBrandId.HasValue)
        {
            root = root.Where(ci => ci.CatalogBrandId == catalogBrandId);
        }

        var totalItems = await root.LongCountAsync();

        var itemsOnPage = await root
            .Skip(pageSize * pageIndex)
            .Take(pageSize)
            .ToListAsync();

        itemsOnPage = ChangeUriPlaceholder(itemsOnPage);

        return new PaginatedItemsViewModel<CatalogItem>(pageIndex, pageSize, totalItems, itemsOnPage);
    }

    /// <summary>
    /// GET api/v1/catalog/catalogtypes
    /// 获取所有的商品类型列表
    /// </summary>
    [HttpGet]
    [Route("catalogtypes")]
    [ProducesResponseType(typeof(List<CatalogType>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<List<CatalogType>>> CatalogTypesAsync()
    {
        return await _catalogContext.CatalogTypes.ToListAsync();
    }

    /// <summary>
    /// GET api/v1/catalog/catalogbrands
    /// 获取所有的商品品牌列表
    /// </summary>
    [HttpGet]
    [Route("catalogbrands")]
    [ProducesResponseType(typeof(List<CatalogBrand>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<List<CatalogBrand>>> CatalogBrandsAsync()
    {
        return await _catalogContext.CatalogBrands.ToListAsync();
    }

    /// <summary>
    /// PUT api/v1/catalog/items
    /// 更新商品信息，如果价格改变则发布价格更改事件
    /// </summary>
    [Route("items")]
    [HttpPut]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.Created)]
    public async Task<ActionResult> UpdateProductAsync([FromBody] CatalogItem productToUpdate)
    {
        // 根据 id 查询现有商品
        var catalogItem = await _catalogContext.CatalogItems.SingleOrDefaultAsync(i => i.Id == productToUpdate.Id);

        if (catalogItem == null)
        {
            return NotFound(new { Message = $"Item with id {productToUpdate.Id} not found." });
        }

        var oldPrice = catalogItem.Price;
        var raiseProductPriceChangedEvent = oldPrice != productToUpdate.Price;

        // 更新当前商品（这里直接替换对象）
        catalogItem = productToUpdate;
        _catalogContext.CatalogItems.Update(catalogItem);

        if (raiseProductPriceChangedEvent)
        {
            // 当商品价格改变时，生成价格变更事件
            var priceChangedEvent = new ProductPriceChangedIntegrationEvent(catalogItem.Id, productToUpdate.Price, oldPrice);
            // 使用本地事务保存更新和事件日志
            await _catalogIntegrationEventService.SaveEventAndCatalogContextChangesAsync(priceChangedEvent);
            // 发布事件到事件总线
            await _catalogIntegrationEventService.PublishThroughEventBusAsync(priceChangedEvent);
        }
        else
        {
            // 若价格未变，仅保存更新
            await _catalogContext.SaveChangesAsync();
        }

        return CreatedAtAction(nameof(ItemByIdAsync), new { id = productToUpdate.Id }, null);
    }

    /// <summary>
    /// POST api/v1/catalog/items
    /// 创建一个新的商品
    /// </summary>
    [Route("items")]
    [HttpPost]
    [ProducesResponseType((int)HttpStatusCode.Created)]
    public async Task<ActionResult> CreateProductAsync([FromBody] CatalogItem product)
    {
        // 创建一个新商品对象（避免直接使用传入的数据）
        var item = new CatalogItem
        {
            CatalogBrandId = product.CatalogBrandId,
            CatalogTypeId = product.CatalogTypeId,
            Description = product.Description,
            Name = product.Name,
            PictureFileName = product.PictureFileName,
            Price = product.Price
        };

        _catalogContext.CatalogItems.Add(item);

        await _catalogContext.SaveChangesAsync();

        return CreatedAtAction(nameof(ItemByIdAsync), new { id = item.Id }, null);
    }

    /// <summary>
    /// DELETE api/v1/catalog/{id}
    /// 根据 id 删除商品
    /// </summary>
    [Route("{id}")]
    [HttpDelete]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult> DeleteProductAsync(int id)
    {
        // 查询商品
        var product = _catalogContext.CatalogItems.SingleOrDefault(x => x.Id == id);

        if (product == null)
        {
            return NotFound();
        }

        // 移除商品记录
        _catalogContext.CatalogItems.Remove(product);

        await _catalogContext.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// 修改商品图片 URI，使用配置中的基础 URL 替换占位符
    /// </summary>
    private List<CatalogItem> ChangeUriPlaceholder(List<CatalogItem> items)
    {
        var baseUri = _settings.PicBaseUrl;
        var azureStorageEnabled = _settings.AzureStorageEnabled;

        foreach (var item in items)
        {
            item.FillProductUrl(baseUri, azureStorageEnabled: azureStorageEnabled);
        }

        return items;
    }
}
