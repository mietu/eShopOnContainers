namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.Commands;

using Microsoft.eShopOnContainers.Services.Ordering.Domain.AggregatesModel.OrderAggregate;
using static Microsoft.eShopOnContainers.Services.Ordering.API.Application.Commands.CreateOrderCommand;

/// <summary>
/// 定义创建订单草稿命令的处理程序，负责将购物篮中的商品转换成订单草稿
/// </summary>
public class CreateOrderDraftCommandHandler : IRequestHandler<CreateOrderDraftCommand, OrderDraftDTO>
{
    // 注入的订单仓储（当前未使用，但可能用于后续持久化订单）
    private readonly IOrderRepository _orderRepository;
    // 注入的身份验证服务，用于获取当前用户信息
    private readonly IIdentityService _identityService;
    // 注入的中介者，用于协调发送其他命令或事件
    private readonly IMediator _mediator;

    /// <summary>
    /// 构造函数，依赖注入所需的服务
    /// </summary>
    /// <param name="mediator">中介者实例</param>
    /// <param name="identityService">身份验证服务实例</param>
    public CreateOrderDraftCommandHandler(IMediator mediator, IIdentityService identityService)
    {
        // 如果通过DI注入的服务为null，则抛出异常
        _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>
    /// 处理创建订单草稿的命令，实现将购物篮中的商品转换为订单草稿数据传输对象
    /// </summary>
    /// <param name="message">创建草稿订单的命令，包含买家信息和购物篮项</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>返回订单草稿的数据传输对象 OrderDraftDTO</returns>
    public Task<OrderDraftDTO> Handle(CreateOrderDraftCommand message, CancellationToken cancellationToken)
    {
        // 创建一个新的订单草稿实例
        var order = Order.NewDraft();

        // 将所有购物篮项转换成订单项数据传输对象（DTO）
        var orderItems = message.Items.Select(i => i.ToOrderItemDTO());

        // 将每个订单项添加到订单草稿中
        foreach (var item in orderItems)
        {
            order.AddOrderItem(
                item.ProductId,    // 产品ID
                item.ProductName,  // 产品名称
                item.UnitPrice,    // 产品单价
                item.Discount,     // 折扣金额
                item.PictureUrl,   // 产品图片URL
                item.Units         // 数量
            );
        }

        // 返回封装后的订单草稿DTO
        return Task.FromResult(OrderDraftDTO.FromOrder(order));
    }
}


/// <summary>
/// 定义订单草稿数据传输对象（DTO），用于返回订单草稿数据
/// </summary>
public record OrderDraftDTO
{
    // 订单中所有的订单项
    public IEnumerable<OrderItemDTO> OrderItems { get; init; }
    // 订单的总金额
    public decimal Total { get; init; }

    /// <summary>
    /// 从订单域模型转换为订单草稿DTO
    /// </summary>
    /// <param name="order">订单域模型</param>
    /// <returns>订单草稿DTO</returns>
    public static OrderDraftDTO FromOrder(Order order)
    {
        return new OrderDraftDTO()
        {
            // 将每个订单项转换为订单项DTO，包含必要的字段信息
            OrderItems = order.OrderItems.Select(oi => new OrderItemDTO
            {
                Discount = oi.GetCurrentDiscount(),              // 当前折扣
                ProductId = oi.ProductId,                          // 产品ID
                UnitPrice = oi.GetUnitPrice(),                     // 单价
                PictureUrl = oi.GetPictureUri(),                   // 图片链接
                Units = oi.GetUnits(),                             // 数量
                ProductName = oi.GetOrderItemProductName()         // 产品名称
            }),
            // 计算订单总金额
            Total = order.GetTotal()
        };
    }
}
