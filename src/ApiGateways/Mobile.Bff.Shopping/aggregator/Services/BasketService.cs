namespace Microsoft.eShopOnContainers.Mobile.Shopping.HttpAggregator.Services;

/// <summary>
/// BasketService 用于与 gRPC Basket 服务进行通信，提供获取与更新购物篮数据的方法。
/// </summary>
public class BasketService : IBasketService
{
    // gRPC 客户端：用于调用 Basket 服务
    private readonly Basket.BasketClient _basketClient;
    // 日志记录器
    private readonly ILogger<BasketService> _logger;

    /// <summary>
    /// 通过依赖注入初始化 BasketService。
    /// </summary>
    /// <param name="basketClient">gRPC Basket 客户端</param>
    /// <param name="logger">日志记录器</param>
    public BasketService(Basket.BasketClient basketClient, ILogger<BasketService> logger)
    {
        _basketClient = basketClient;
        _logger = logger;
    }

    /// <summary>
    /// 根据购物篮 ID 获取购物篮数据。
    /// </summary>
    /// <param name="id">购物篮唯一标识</param>
    /// <returns>返回转换后的 BasketData 数据</returns>
    public async Task<BasketData> GetByIdAsync(string id)
    {
        // 记录请求日志
        _logger.LogDebug("grpc client created, request = {@id}", id);
        // 调用 gRPC 服务获取购物篮数据
        var response = await _basketClient.GetBasketByIdAsync(new BasketRequest { Id = id });
        // 记录响应日志
        _logger.LogDebug("grpc response {@response}", response);

        // 映射 gRPC 响应到本地数据模型并返回
        return MapToBasketData(response);
    }

    /// <summary>
    /// 更新购物篮数据。
    /// </summary>
    /// <param name="currentBasket">当前的购物篮数据</param>
    public async Task UpdateAsync(BasketData currentBasket)
    {
        // 记录当前购物篮数据的更新日志
        _logger.LogDebug("Grpc update basket currentBasket {@currentBasket}", currentBasket);
        // 将本地购物篮数据模型转换为 gRPC 请求模型
        var request = MapToCustomerBasketRequest(currentBasket);
        // 记录转换后的请求日志
        _logger.LogDebug("Grpc update basket request {@request}", request);

        // 调用 gRPC 服务更新购物篮信息
        await _basketClient.UpdateBasketAsync(request);
    }

    /// <summary>
    /// 将 gRPC 的 CustomerBasketResponse 对象映射为本地的 BasketData 数据模型。
    /// </summary>
    /// <param name="customerBasketRequest">gRPC 返回的数据</param>
    /// <returns>映射后的 BasketData 对象</returns>
    private BasketData MapToBasketData(CustomerBasketResponse customerBasketRequest)
    {
        if (customerBasketRequest == null)
        {
            return null;
        }

        // 创建本地 BasketData 对象，并设置 BuyerId
        var map = new BasketData
        {
            BuyerId = customerBasketRequest.Buyerid
        };

        // 遍历 gRPC 返回的商品列表，转换为本地的 BasketDataItem 数据模型
        customerBasketRequest.Items.ToList().ForEach(item => map.Items.Add(new BasketDataItem
        {
            Id = item.Id,
            OldUnitPrice = (decimal)item.Oldunitprice,
            PictureUrl = item.Pictureurl,
            ProductId = item.Productid,
            ProductName = item.Productname,
            Quantity = item.Quantity,
            UnitPrice = (decimal)item.Unitprice
        }));

        return map;
    }

    /// <summary>
    /// 将本地的 BasketData 对象映射为 gRPC 请求所需的 CustomerBasketRequest 对象。
    /// </summary>
    /// <param name="basketData">本地购物篮数据</param>
    /// <returns>映射后的 gRPC 请求数据</returns>
    private CustomerBasketRequest MapToCustomerBasketRequest(BasketData basketData)
    {
        if (basketData == null)
        {
            return null;
        }

        // 创建 gRPC 请求对象，并设置 Buyerid
        var map = new CustomerBasketRequest
        {
            Buyerid = basketData.BuyerId
        };

        // 遍历本地购物篮中的商品项，转换为 gRPC 所需的 BasketItemResponse 数据模型
        basketData.Items.ToList().ForEach(item => map.Items.Add(new BasketItemResponse
        {
            Id = item.Id,
            Oldunitprice = (double)item.OldUnitPrice,
            Pictureurl = item.PictureUrl,
            Productid = item.ProductId,
            Productname = item.ProductName,
            Quantity = item.Quantity,
            Unitprice = (double)item.UnitPrice
        }));

        return map;
    }
}
