namespace GrpcBasket;

// BasketService 继承自 gRPC 自动生成的 Basket.BasketBase 类，实现购物篮服务的 gRPC 接口
public class BasketService : Basket.BasketBase
{
    private readonly IBasketRepository _repository;
    private readonly ILogger<BasketService> _logger;

    // 构造函数，通过依赖注入获取 IBasketRepository 和 ILogger 实例
    public BasketService(IBasketRepository repository, ILogger<BasketService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    // 允许匿名调用：根据购物篮请求ID获取购物篮数据
    [AllowAnonymous]
    public override async Task<CustomerBasketResponse> GetBasketById(BasketRequest request, ServerCallContext context)
    {
        // 记录 gRPC 调用信息，包含方法名和购物篮ID
        _logger.LogInformation("Begin grpc call from method {Method} for basket id {Id}", context.Method, request.Id);

        // 从仓储中异步获取购物篮数据
        var data = await _repository.GetBasketAsync(request.Id);

        if (data != null)
        {
            // 如果找到购物篮，设置上下文状态为 OK
            context.Status = new Status(StatusCode.OK, $"Basket with id {request.Id} do exist");
            // 将 CustomerBasket 转换为 CustomerBasketResponse 并返回
            return MapToCustomerBasketResponse(data);
        }
        else
        {
            // 如果没有找到对应购物篮，设置上下文状态为 NotFound
            context.Status = new Status(StatusCode.NotFound, $"Basket with id {request.Id} do not exist");
        }

        // 返回一个空的响应对象
        return new CustomerBasketResponse();
    }

    // 更新购物篮数据的方法，接收 CustomerBasketRequest 请求对象
    public override async Task<CustomerBasketResponse> UpdateBasket(CustomerBasketRequest request, ServerCallContext context)
    {
        // 记录日志，记录调用者的买家ID
        _logger.LogInformation("Begin grpc call BasketService.UpdateBasketAsync for buyer id {Buyerid}", request.Buyerid);

        // 将 gRPC 请求数据映射为本地的 CustomerBasket 对象
        var customerBasket = MapToCustomerBasket(request);

        // 调用仓储更新购物篮数据
        var response = await _repository.UpdateBasketAsync(customerBasket);

        if (response != null)
        {
            // 如果更新成功，则将结果映射为响应对象并返回
            return MapToCustomerBasketResponse(response);
        }

        // 如果没有找到对应的购物篮，设置上下文状态为 NotFound
        context.Status = new Status(StatusCode.NotFound, $"Basket with buyer id {request.Buyerid} do not exist");

        return null;
    }

    // 私有方法：将 CustomerBasket 对象映射为 gRPC 的响应对象 CustomerBasketResponse
    private CustomerBasketResponse MapToCustomerBasketResponse(CustomerBasket customerBasket)
    {
        // 初始化响应对象，设置买家ID
        var response = new CustomerBasketResponse
        {
            Buyerid = customerBasket.BuyerId
        };

        // 遍历购物篮中的每一项，转换为 BasketItemResponse 并添加到响应集合中
        customerBasket.Items.ForEach(item => response.Items.Add(new BasketItemResponse
        {
            Id = item.Id,
            Oldunitprice = (double)item.OldUnitPrice,
            Pictureurl = item.PictureUrl,
            Productid = item.ProductId,
            Productname = item.ProductName,
            Quantity = item.Quantity,
            Unitprice = (double)item.UnitPrice
        }));

        return response;
    }

    // 私有方法：将 gRPC 的请求对象 CustomerBasketRequest 映射为本地的 CustomerBasket 对象
    private CustomerBasket MapToCustomerBasket(CustomerBasketRequest customerBasketRequest)
    {
        // 初始化 CustomerBasket 对象，并设置买家ID
        var response = new CustomerBasket
        {
            BuyerId = customerBasketRequest.Buyerid
        };

        // 遍历请求中的每一项，将其转换为本地 BasketItem 并添加到购物篮中
        customerBasketRequest.Items.ToList().ForEach(item => response.Items.Add(new BasketItem
        {
            Id = item.Id,
            OldUnitPrice = (decimal)item.Oldunitprice,
            PictureUrl = item.Pictureurl,
            ProductId = item.Productid,
            ProductName = item.Productname,
            Quantity = item.Quantity,
            UnitPrice = (decimal)item.Unitprice
        }));

        return response;
    }
}
