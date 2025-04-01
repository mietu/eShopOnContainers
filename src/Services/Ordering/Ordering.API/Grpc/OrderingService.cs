namespace GrpcOrdering;

// OrderingService 继承自 OrderingGrpcBase，是一个 gRPC 服务实现类，用于处理订单草稿的创建请求
public class OrderingService : OrderingGrpc.OrderingGrpcBase
{
    // 依赖注入的 IMediator 用于发送命令，ILogger 用于日志记录
    private readonly IMediator _mediator;
    private readonly ILogger<OrderingService> _logger;

    // 构造函数，初始化依赖项
    public OrderingService(IMediator mediator, ILogger<OrderingService> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    // 实现 gRPC 方法：从篮子数据创建订单草稿
    public override async Task<OrderDraftDTO> CreateOrderDraftFromBasketData(CreateOrderDraftCommand createOrderDraftCommand, ServerCallContext context)
    {
        // 记录方法调用信息
        _logger.LogInformation("Begin grpc call from method {Method} for ordering get order draft {CreateOrderDraftCommand}", context.Method, createOrderDraftCommand);
        _logger.LogTrace(
            "----- Sending command: {CommandName} - {IdProperty}: {CommandId} ({@Command})",
            createOrderDraftCommand.GetGenericTypeName(),
            nameof(createOrderDraftCommand.BuyerId),
            createOrderDraftCommand.BuyerId,
            createOrderDraftCommand);

        // 将 gRPC 请求数据映射为应用层命令
        var command = new AppCommand.CreateOrderDraftCommand(
                            createOrderDraftCommand.BuyerId,
                            this.MapBasketItems(createOrderDraftCommand.Items));

        // 发送命令，并等待处理结果
        var data = await _mediator.Send(command);

        // 如果返回数据不为空，则设置响应状态码为 OK，返回映射后的结果
        if (data != null)
        {
            context.Status = new Status(StatusCode.OK, $" ordering get order draft {createOrderDraftCommand} do exist");

            return this.MapResponse(data);
        }
        else
        {
            // 如果无数据，则设置响应状态码为 NotFound，返回空订单草稿 DTO
            context.Status = new Status(StatusCode.NotFound, $" ordering get order draft {createOrderDraftCommand} do not exist");
        }

        return new OrderDraftDTO();
    }

    // 将应用层的 OrderDraftDTO 转换为 gRPC 的 OrderDraftDTO
    public OrderDraftDTO MapResponse(AppCommand.OrderDraftDTO order)
    {
        // 创建返回结果，并设置总金额
        var result = new OrderDraftDTO()
        {
            Total = (double)order.Total,
        };

        // 逐个映射订单项，将其添加到结果中
        order.OrderItems.ToList().ForEach(i => result.OrderItems.Add(new OrderItemDTO()
        {
            Discount = (double)i.Discount,
            PictureUrl = i.PictureUrl,
            ProductId = i.ProductId,
            ProductName = i.ProductName,
            UnitPrice = (double)i.UnitPrice,
            Units = i.Units,
        }));

        return result;
    }

    // 将 gRPC 范围内的 BasketItem 集合映射成应用层的 BasketItem 集合
    public IEnumerable<ApiModels.BasketItem> MapBasketItems(RepeatedField<BasketItem> items)
    {
        return items.Select(x => new ApiModels.BasketItem()
        {
            Id = x.Id,
            ProductId = x.ProductId,
            ProductName = x.ProductName,
            UnitPrice = (decimal)x.UnitPrice,
            OldUnitPrice = (decimal)x.OldUnitPrice,
            Quantity = x.Quantity,
            PictureUrl = x.PictureUrl,
        });
    }
}
