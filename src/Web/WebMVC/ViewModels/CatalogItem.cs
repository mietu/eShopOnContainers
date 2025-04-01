namespace Microsoft.eShopOnContainers.WebMVC.ViewModels;

/// <summary>
/// 表示目录中的商品项。
/// 实现为不可变记录类型，创建后所有属性都是只读的。
/// 用作 MVC 应用程序中的视图模型。
/// </summary>
public record CatalogItem
{
    /// <summary>
    /// 获取或初始化商品的唯一标识符。
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// 获取或初始化商品的名称。
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// 获取或初始化商品的详细描述。
    /// </summary>
    public string Description { get; init; }

    /// <summary>
    /// 获取或初始化商品的价格。
    /// </summary>
    public decimal Price { get; init; }

    /// <summary>
    /// 获取或初始化商品图片的 URI。
    /// </summary>
    public string PictureUri { get; init; }

    /// <summary>
    /// 获取或初始化商品品牌的唯一标识符。
    /// </summary>
    public int CatalogBrandId { get; init; }

    /// <summary>
    /// 获取或初始化商品品牌的名称。
    /// </summary>
    public string CatalogBrand { get; init; }

    /// <summary>
    /// 获取或初始化商品类型的唯一标识符。
    /// </summary>
    public int CatalogTypeId { get; init; }

    /// <summary>
    /// 获取或初始化商品类型的名称。
    /// </summary>
    public string CatalogType { get; init; }
}
