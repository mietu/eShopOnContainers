namespace Microsoft.eShopOnContainers.WebMVC.ViewModels;

/// <summary>
/// 表示商品目录的视图模型，用于分页展示商品数据
/// </summary>
public record Catalog
{
    /// <summary>
    /// 获取当前页索引，用于分页导航
    /// </summary>
    public int PageIndex { get; init; }

    /// <summary>
    /// 获取每页显示的商品数量
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// 获取商品的总数量
    /// </summary>
    public int Count { get; init; }

    /// <summary>
    /// 获取当前页的商品项列表
    /// </summary>
    public List<CatalogItem> Data { get; init; }
}
