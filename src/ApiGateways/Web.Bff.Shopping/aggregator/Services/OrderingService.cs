namespace Microsoft.eShopOnContainers.Web.Shopping.HttpAggregator.Services;

/// <summary>
/// OrderingService 用于调用 gRPC 服务生成订单草稿。
/// </summary>
public class OrderingService : IOrderingService
{
    // gRPC 客户端，用于与订单服务通信
    private readonly OrderingGrpc.OrderingGrpcClient _orderingGrpcClient;
    // 日志记录器，用于记录调试信息
    private readonly ILogger<OrderingService> _logger;

    /// <summary>
    /// 构造函数，通过依赖注入初始化 gRPC 客户端和日志记录器
    /// </summary>
    /// <param name="orderingGrpcClient">gRPC 客户端实例</param>
    /// <param name="logger">日志记录器实例</param>
    public OrderingService(OrderingGrpc.OrderingGrpcClient orderingGrpcClient, ILogger<OrderingService> logger)
    {
        _orderingGrpcClient = orderingGrpcClient;
        _logger = logger;
    }

    /// <summary>
    /// 根据篮子数据生成订单草稿
    /// </summary>
    /// <param name="basketData">购物篮中的数据</param>
    /// <returns>生成的订单数据</returns>
    public async Task<OrderData> GetOrderDraftAsync(BasketData basketData)
    {
        // 记录接收到的购物篮数据
        _logger.LogDebug(" grpc client created, basketData={@basketData}", basketData);

        // 将购物篮数据映射为创建订单草稿的命令对象
        var command = MapToOrderDraftCommand(basketData);
        // 调用 gRPC 服务方法生成订单草稿
        var response = await _orderingGrpcClient.CreateOrderDraftFromBasketDataAsync(command);
        // 记录 gRPC 响应信息
        _logger.LogDebug(" grpc response: {@response}", response);

        // 将 gRPC 响应转换为 OrderData 返回
        return MapToResponse(response, basketData);
    }

    /// <summary>
    /// 将 gRPC 返回的订单草稿 DTO 映射为客户端使用的 OrderData 对象
    /// </summary>
    /// <param name="orderDraft">gRPC 返回的订单草稿 DTO</param>
    /// <param name="basketData">对应的购物篮数据</param>
    /// <returns>映射后的 OrderData 对象</returns>
    private OrderData MapToResponse(GrpcOrdering.OrderDraftDTO orderDraft, BasketData basketData)
    {
        // 如果响应为空，则直接返回 null
        if (orderDraft == null)
        {
            return null;
        }

        // 初始化 OrderData 对象，并设置买家和总金额
        var data = new OrderData
        {
            Buyer = basketData.BuyerId,
            Total = (decimal)orderDraft.Total,
        };

        // 遍历订单中的每个商品项，添加到 OrderData 中
        orderDraft.OrderItems.ToList().ForEach(o => data.OrderItems.Add(new OrderItemData
        {
            Discount = (decimal)o.Discount,
            PictureUrl = o.PictureUrl,
            ProductId = o.ProductId,
            ProductName = o.ProductName,
            UnitPrice = (decimal)o.UnitPrice,
            Units = o.Units,
        }));

        return data;
    }

    /// <summary>
    /// 将购物篮数据映射为创建订单草稿的命令对象
    /// </summary>
    /// <param name="basketData">购物篮数据</param>
    /// <returns>生成的 CreateOrderDraftCommand 对象</returns>
    private CreateOrderDraftCommand MapToOrderDraftCommand(BasketData basketData)
    {
        // 初始化命令对象，并设置买家标识
        var command = new CreateOrderDraftCommand
        {
            BuyerId = basketData.BuyerId,
        };

        // 将购物篮中的每个商品映射为命令对应的商品项
        basketData.Items.ForEach(i => command.Items.Add(new BasketItem
        {
            Id = i.Id,
            OldUnitPrice = (double)i.OldUnitPrice,
            PictureUrl = i.PictureUrl,
            ProductId = i.ProductId,
            ProductName = i.ProductName,
            Quantity = i.Quantity,
            UnitPrice = (double)i.UnitPrice,
        }));

        return command;
    }
}
