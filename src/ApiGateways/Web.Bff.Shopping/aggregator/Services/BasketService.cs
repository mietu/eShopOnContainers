namespace Microsoft.eShopOnContainers.Web.Shopping.HttpAggregator.Services;

// BasketService 类负责与 Basket gRPC 服务进行通信，
// 实现 IBasketService 接口提供获取和更新购物篮数据的功能。
public class BasketService : IBasketService
{
    // BasketClient 用于调用 gRPC 服务接口
    private readonly Basket.BasketClient _basketClient;
    // 用于日志记录
    private readonly ILogger<BasketService> _logger;

    // 构造函数依赖注入 BasketClient 和 ILogger
    public BasketService(Basket.BasketClient basketClient, ILogger<BasketService> logger)
    {
        _basketClient = basketClient;
        _logger = logger;
    }

    // 通过购物篮 ID 获取购物篮数据的方法
    public async Task<BasketData> GetByIdAsync(string id)
    {
        // 记录调试日志，展示请求使用的 id
        _logger.LogDebug("grpc client created, request = {@id}", id);
        // 调用 gRPC 服务获取购物篮数据
        var response = await _basketClient.GetBasketByIdAsync(new BasketRequest { Id = id });
        // 记录调试日志，展示从 gRPC 返回的响应数据
        _logger.LogDebug("grpc response {@response}", response);

        // 将 gRPC 返回的数据转换成本地 BasketData 对象
        return MapToBasketData(response);
    }

    // 更新购物篮数据的方法
    public async Task UpdateAsync(BasketData currentBasket)
    {
        // 记录调试日志，展示当前购物篮数据
        _logger.LogDebug("Grpc update basket currentBasket {@currentBasket}", currentBasket);
        // 将本地 BasketData 转换为 gRPC 请求对象 CustomerBasketRequest
        var request = MapToCustomerBasketRequest(currentBasket);
        // 记录调试日志，展示转换后的请求对象
        _logger.LogDebug("Grpc update basket request {@request}", request);

        // 调用 gRPC 服务更新购物篮数据
        await _basketClient.UpdateBasketAsync(request);
    }

    // 将 gRPC 返回的数据 (CustomerBasketResponse) 映射到本地 BasketData 对象
    private BasketData MapToBasketData(CustomerBasketResponse customerBasketRequest)
    {
        if (customerBasketRequest == null)
        {
            return null;
        }

        // 初始化 BasketData 对象并设置 BuyerId
        var map = new BasketData
        {
            BuyerId = customerBasketRequest.Buyerid
        };

        // 遍历响应中的购物篮项，并转换每一项
        customerBasketRequest.Items.ToList().ForEach(item =>
        {
            if (item.Id != null)
            {
                // 添加转换后的 BasketDataItem 对象到 BasketData.Items 集合中
                map.Items.Add(new BasketDataItem
                {
                    Id = item.Id,
                    OldUnitPrice = (decimal)item.Oldunitprice,
                    PictureUrl = item.Pictureurl,
                    ProductId = item.Productid,
                    ProductName = item.Productname,
                    Quantity = item.Quantity,
                    UnitPrice = (decimal)item.Unitprice
                });
            }
        });

        return map;
    }

    // 将本地的 BasketData 对象映射到 gRPC 请求的 CustomerBasketRequest 对象
    private CustomerBasketRequest MapToCustomerBasketRequest(BasketData basketData)
    {
        if (basketData == null)
        {
            return null;
        }

        // 初始化 CustomerBasketRequest 对象并设置 BuyerId
        var map = new CustomerBasketRequest
        {
            Buyerid = basketData.BuyerId
        };

        // 遍历 BasketData 中每个购物篮项，并转换成对应的 BasketItemResponse 对象
        basketData.Items.ToList().ForEach(item =>
        {
            if (item.Id != null)
            {
                // 添加转换后的 BasketItemResponse 到请求对象的 Items 集合中
                map.Items.Add(new BasketItemResponse
                {
                    Id = item.Id,
                    Oldunitprice = (double)item.OldUnitPrice,
                    Pictureurl = item.PictureUrl,
                    Productid = item.ProductId,
                    Productname = item.ProductName,
                    Quantity = item.Quantity,
                    Unitprice = (double)item.UnitPrice
                });
            }
        });

        return map;
    }
}
