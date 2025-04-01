namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.Models;

using System.Collections.Generic;
using static Microsoft.eShopOnContainers.Services.Ordering.API.Application.Commands.CreateOrderCommand;

// 扩展方法类，用于将 BasketItem 实例转换为 OrderItemDTO 实例
public static class BasketItemExtensions
{
    /// <summary>
    /// 将一组 BasketItem 转换为 OrderItemDTO 集合
    /// </summary>
    /// <param name="basketItems">BasketItem 集合</param>
    /// <returns>转换后的 OrderItemDTO 集合</returns>
    public static IEnumerable<OrderItemDTO> ToOrderItemsDTO(this IEnumerable<BasketItem> basketItems)
    {
        // 遍历所有 BasketItem 对象
        foreach (var item in basketItems)
        {
            // 转换单个 BasketItem 为 OrderItemDTO，并返回
            yield return item.ToOrderItemDTO();
        }
    }

    /// <summary>
    /// 将单个 BasketItem 转换为 OrderItemDTO 对象
    /// </summary>
    /// <param name="item">单个 BasketItem 对象</param>
    /// <returns>转换后的 OrderItemDTO 对象</returns>
    public static OrderItemDTO ToOrderItemDTO(this BasketItem item)
    {
        // 创建并返回 OrderItemDTO 实例，复制对应属性值
        return new OrderItemDTO()
        {
            ProductId = item.ProductId,        // 商品ID
            ProductName = item.ProductName,      // 商品名称
            PictureUrl = item.PictureUrl,        // 商品图片URL
            UnitPrice = item.UnitPrice,          // 单价
            Units = item.Quantity                // 购买数量
        };
    }
}
