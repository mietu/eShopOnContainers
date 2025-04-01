namespace Microsoft.eShopOnContainers.Mobile.Shopping.HttpAggregator.Models;

/// <summary>
/// 表示目录中的一项物品。
/// </summary>
public class CatalogItem
{
    /// <summary>
    /// 获取或设置物品的唯一标识符。
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 获取或设置物品的名称。
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 获取或设置物品的价格。
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// 获取或设置物品图片的URI。
    /// </summary>
    public string PictureUri { get; set; }
}
