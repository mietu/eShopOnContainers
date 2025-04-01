namespace Microsoft.eShopOnContainers.Mobile.Shopping.HttpAggregator.Controllers;

[Route("api/v1/[controller]")]
[Authorize]
[ApiController]
public class BasketController : ControllerBase
{
    private readonly ICatalogService _catalog;
    private readonly IBasketService _basket;

    // 构造函数，用于注入 ICatalogService 和 IBasketService 依赖项
    public BasketController(ICatalogService catalogService, IBasketService basketService)
    {
        _catalog = catalogService;
        _basket = basketService;
    }

    /// <summary>
    /// 更新整个购物篮
    /// 支持 POST 和 PUT 请求
    /// </summary>
    /// <param name="data">更新购物篮请求数据</param>
    /// <returns>更新后的购物篮数据</returns>
    [HttpPost]
    [HttpPut]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(BasketData), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<BasketData>> UpdateAllBasketAsync([FromBody] UpdateBasketRequest data)
    {
        // 校验请求数据是否包含购物篮行项目
        if (data.Items == null || !data.Items.Any())
        {
            return BadRequest("Need to pass at least one basket line");
        }

        // 获取当前购物篮，如果不存在，则新建一个购物篮数据实例
        var basket = await _basket.GetByIdAsync(data.BuyerId) ?? new BasketData(data.BuyerId);
        // 从目录中获取所有相关的商品信息
        var catalogItems = await _catalog.GetCatalogItemsAsync(data.Items.Select(x => x.ProductId));

        // 对请求数据中的购物篮项目进行分组，以避免同一商品的重复添加，
        // 并计算每种商品的总数量。
        var itemsCalculated = data
                .Items
                .GroupBy(x => x.ProductId, x => x, (k, i) => new { productId = k, items = i })
                .Select(groupedItem =>
                {
                    var item = groupedItem.items.First();
                    item.Quantity = groupedItem.items.Sum(i => i.Quantity);
                    return item;
                });

        // 遍历所有更新的购物篮行项目
        foreach (var bitem in itemsCalculated)
        {
            // 查找对应的目录商品，如果找不到，则返回错误
            var catalogItem = catalogItems.SingleOrDefault(ci => ci.Id == bitem.ProductId);
            if (catalogItem == null)
            {
                return BadRequest($"Basket refers to a non-existing catalog item ({bitem.ProductId})");
            }

            // 尝试在当前购物篮中查找该商品
            var itemInBasket = basket.Items.FirstOrDefault(x => x.ProductId == bitem.ProductId);
            if (itemInBasket == null)
            {
                // 如果当前购物篮中不存在，则添加新商品项
                basket.Items.Add(new BasketDataItem()
                {
                    Id = bitem.Id,
                    ProductId = catalogItem.Id,
                    ProductName = catalogItem.Name,
                    PictureUrl = catalogItem.PictureUri,
                    UnitPrice = catalogItem.Price,
                    Quantity = bitem.Quantity
                });
            }
            else
            {
                // 如果存在，则更新数量信息
                itemInBasket.Quantity = bitem.Quantity;
            }
        }

        // 更新购物篮数据持久化存储
        await _basket.UpdateAsync(basket);

        return basket;
    }

    /// <summary>
    /// 更新购物篮中各商品项的数量
    /// </summary>
    /// <param name="data">更新购物篮各商品数量请求数据</param>
    /// <returns>更新后的购物篮数据</returns>
    [HttpPut]
    [Route("items")]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(BasketData), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<BasketData>> UpdateQuantitiesAsync([FromBody] UpdateBasketItemsRequest data)
    {
        if (!data.Updates.Any())
        {
            return BadRequest("No updates sent");
        }

        // 获取当前购物篮
        var currentBasket = await _basket.GetByIdAsync(data.BasketId);
        if (currentBasket == null)
        {
            return BadRequest($"Basket with id {data.BasketId} not found.");
        }

        // 遍历每个更新项，并更新对应购物篮中的商品数量
        foreach (var update in data.Updates)
        {
            var basketItem = currentBasket.Items.SingleOrDefault(bitem => bitem.Id == update.BasketItemId);

            if (basketItem == null)
            {
                return BadRequest($"Basket item with id {update.BasketItemId} not found");
            }

            basketItem.Quantity = update.NewQty;
        }

        // 保存更新后的购物篮数据
        await _basket.UpdateAsync(currentBasket);

        return currentBasket;
    }

    /// <summary>
    /// 向购物篮中添加单个商品项
    /// </summary>
    /// <param name="data">添加购物篮商品请求数据</param>
    /// <returns>操作结果</returns>
    [HttpPost]
    [Route("items")]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<ActionResult> AddBasketItemAsync([FromBody] AddBasketItemRequest data)
    {
        // 校验请求有效性
        if (data == null || data.Quantity == 0)
        {
            return BadRequest("Invalid payload");
        }

        // 第一步：通过目录服务获取商品详情
        var item = await _catalog.GetCatalogItemAsync(data.CatalogItemId);

        // 第二步：获取当前的购物篮状态，如果不存在，则新建一个购物篮
        var currentBasket = (await _basket.GetByIdAsync(data.BasketId)) ?? new BasketData(data.BasketId);

        // 第三步：将新商品合并加入到购物篮中
        currentBasket.Items.Add(new BasketDataItem()
        {
            UnitPrice = item.Price,
            PictureUrl = item.PictureUri,
            ProductId = item.Id,
            ProductName = item.Name,
            Quantity = data.Quantity,
            Id = Guid.NewGuid().ToString()
        });

        // 第四步：更新购物篮数据
        await _basket.UpdateAsync(currentBasket);

        return Ok();
    }
}
