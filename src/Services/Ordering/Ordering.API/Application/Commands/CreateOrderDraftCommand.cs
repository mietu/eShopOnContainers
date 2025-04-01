namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.Commands;
using Microsoft.eShopOnContainers.Services.Ordering.API.Application.Models;

// ICommand 接口通常由 Mediator 模式使用，这里 IRequest<OrderDraftDTO> 定义了命令返回一个 OrderDraftDTO 对象。
public class CreateOrderDraftCommand : IRequest<OrderDraftDTO>
{
    // 表示购买者的标识
    public string BuyerId { get; private set; }

    // 表示购物篮中的商品集合
    public IEnumerable<BasketItem> Items { get; private set; }

    // 构造函数，初始化购买者标识和购物篮项集合
    public CreateOrderDraftCommand(string buyerId, IEnumerable<BasketItem> items)
    {
        BuyerId = buyerId;  // 设置购买者标识
        Items = items;      // 设置购物篮中的商品集合
    }
}
