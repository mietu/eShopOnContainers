namespace Microsoft.eShopOnContainers.Web.Shopping.HttpAggregator.Controllers;

// Controller 提供对购物篮操作的 API（例如更新购物篮、修改数量、添加商品等）
[Route("api/v1/[controller]")]
[Authorize]
[ApiController]
public class BasketController : ControllerBase
{
    // 注入目录服务和购物篮服务接口，用于获取商品信息和更新购物篮
    private readonly ICatalogService _catalog;
    private readonly IBasketService _basket;

    // 构造函数用于依赖注入服务
    public BasketController(ICatalogService catalogService, IBasketService basketService)
    {
        _catalog = catalogService;
        _basket = basketService;
    }

    // 更新整个购物篮的方法，同时支持 POST 与 PUT 请求
    [HttpPost]
    [HttpPut]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(BasketData), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<BasketData>> UpdateAllBasketAsync([FromBody] UpdateBasketRequest data)
    {
        // 检查传入的购物篮明细是否为空或者没有行
        if (data.Items == null || !data.Items.Any())
        {
            return BadRequest("Need to pass at least one basket line");
        }

        // 从服务中获取当前的购物篮，如果不存在则创建一个新的购物篮实例
        var basket = await _basket.GetByIdAsync(data.BuyerId) ?? new BasketData(data.BuyerId);
        // 根据传入的商品ID获取对应的商品详情（目录信息）
        var catalogItems = await _catalog.GetCatalogItemsAsync(data.Items.Select(x => x.ProductId));

        // 对商品进行分组，避免重复，并计算每个商品的总数量
        var itemsCalculated = data
            .Items
            .GroupBy(x => x.ProductId, x => x, (k, i) => new { productId = k, items = i })
            .Select(groupedItem =>
            {
                var item = groupedItem.items.First();
                // 求和所有相同商品的数量
                item.Quantity = groupedItem.items.Sum(i => i.Quantity);
                return item;
            });

        // 遍历每个更新的购物篮商品行
        foreach (var bitem in itemsCalculated)
        {
            // 判断商品是否存在于目录中
            var catalogItem = catalogItems.SingleOrDefault(ci => ci.Id == bitem.ProductId);
            if (catalogItem == null)
            {
                return BadRequest($"Basket refers to a non-existing catalog item ({bitem.ProductId})");
            }

            // 检查购物篮中是否已包含该商品
            var itemInBasket = basket.Items.FirstOrDefault(x => x.ProductId == bitem.ProductId);
            if (itemInBasket == null)
            {
                // 如果购物篮中没有, 则添加新的购物篮项
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
                // 如果已经存在，则更新数量
                itemInBasket.Quantity = bitem.Quantity;
            }
        }

        // 更新购物篮状态
        await _basket.UpdateAsync(basket);

        return basket;
    }

    // 更新购物篮中具体商品数量的方法，使用 PUT 请求
    [HttpPut]
    [Route("items")]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(BasketData), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<BasketData>> UpdateQuantitiesAsync([FromBody] UpdateBasketItemsRequest data)
    {
        // 检查是否有更新项发送过来
        if (!data.Updates.Any())
        {
            return BadRequest("No updates sent");
        }

        // 获取当前的购物篮
        var currentBasket = await _basket.GetByIdAsync(data.BasketId);
        if (currentBasket == null)
        {
            return BadRequest($"Basket with id {data.BasketId} not found.");
        }

        // 遍历每个更新项，并更新购物篮中对应的商品数量
        foreach (var update in data.Updates)
        {
            var basketItem = currentBasket.Items.SingleOrDefault(bitem => bitem.Id == update.BasketItemId);
            if (basketItem == null)
            {
                return BadRequest($"Basket item with id {update.BasketItemId} not found");
            }
            basketItem.Quantity = update.NewQty;
        }

        // 保存更新后的购物篮状态
        await _basket.UpdateAsync(currentBasket);

        return currentBasket;
    }

    // 添加购物篮商品的方法，使用 POST 请求
    [HttpPost]
    [Route("items")]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<ActionResult> AddBasketItemAsync([FromBody] AddBasketItemRequest data)
    {
        // 检查传入数据是否合法（例如数量不能为0）
        if (data == null || data.Quantity == 0)
        {
            return BadRequest("Invalid payload");
        }

        // 第一步：从目录获取具体商品信息
        var item = await _catalog.GetCatalogItemAsync(data.CatalogItemId);

        // 第二步：获取当前购物篮状态，如不存在则创建一个新的购物篮
        var currentBasket = (await _basket.GetByIdAsync(data.BasketId)) ?? new BasketData(data.BasketId);
        // 第三步：在购物篮中查找是否已存在该商品
        var product = currentBasket.Items.SingleOrDefault(i => i.ProductId == item.Id);
        if (product != null)
        {
            // 如果购物篮中存在，更新对应商品的数量
            product.Quantity += data.Quantity;
        }
        else
        {
            // 如果不存在，则新增一个购物篮项
            currentBasket.Items.Add(new BasketDataItem()
            {
                UnitPrice = item.Price,
                PictureUrl = item.PictureUri,
                ProductId = item.Id,
                ProductName = item.Name,
                Quantity = data.Quantity,
                // 生成一个新的 Id 标识购物篮项
                Id = Guid.NewGuid().ToString()
            });
        }

        // 第五步：更新购物篮状态
        await _basket.UpdateAsync(currentBasket);

        return Ok();
    }
}
