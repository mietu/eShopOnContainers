namespace Microsoft.eShopOnContainers.Mobile.Shopping.HttpAggregator.Services;

/// <summary>
/// OrderingService：用于通过 gRPC 客户端调用订单服务生成订单草稿，同时记录流程日志。
/// </summary>
public class OrderingService : IOrderingService
{
    // gRPC 客户端，用于与订单服务进行通信
    private readonly OrderingGrpc.OrderingGrpcClient _orderingGrpcClient;
    // 日志记录器，用于输出调试等日志信息
    private readonly ILogger<OrderingService> _logger;

    /// <summary>
    /// 构造函数，通过依赖注入初始化 gRPC 客户端和日志记录器
    /// </summary>
    /// <param name="orderingGrpcClient">注入的 gRPC 客户端实例</param>
    /// <param name="logger">注入的日志记录器实例</param>
    public OrderingService(OrderingGrpc.OrderingGrpcClient orderingGrpcClient, ILogger<OrderingService> logger)
    {
        _orderingGrpcClient = orderingGrpcClient;
        _logger = logger;
    }

    /// <summary>
    /// 根据购物篮数据生成订单草稿
    /// </summary>
    /// <param name="basketData">购物篮数据</param>
    /// <returns>生成的订单数据</returns>
    public async Task<OrderData> GetOrderDraftAsync(BasketData basketData)
    {
        // 记录传入的购物篮数据
        _logger.LogDebug("grpc client created, basketData={@basketData}", basketData);

        // 将购物篮数据映射为创建订单草稿的命令
        var command = MapToOrderDraftCommand(basketData);
        // 调用 gRPC 服务创建订单草稿
        var response = await _orderingGrpcClient.CreateOrderDraftFromBasketDataAsync(command);
        // 记录 gRPC 返回结果
        _logger.LogDebug("grpc response: {@response}", response);

        // 将 gRPC 返回的 DTO 转换为 OrderData 对象并返回
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
        // 如果 gRPC 返回的 DTO 为 null，则返回 null
        if (orderDraft == null)
        {
            return null;
        }

        // 根据购物篮中的买家信息和 gRPC 返回的总价格构造 OrderData 对象
        var data = new OrderData
        {
            Buyer = basketData.BuyerId,
            Total = (decimal)orderDraft.Total,
        };

        // 遍历 gRPC 返回的订单项，构造每个订单项的数据，并加入到 OrderData 中
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
        // 根据购物篮数据的买家信息构造初始命令对象
        var command = new CreateOrderDraftCommand
        {
            BuyerId = basketData.BuyerId,
        };

        // 遍历购物篮中的每个商品项，将其映射为 BasketItem 并加入命令对象中
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
