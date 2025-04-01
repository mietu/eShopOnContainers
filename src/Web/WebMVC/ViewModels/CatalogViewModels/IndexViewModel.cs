namespace Microsoft.eShopOnContainers.WebMVC.ViewModels.CatalogViewModels;

/// <summary>
/// 目录首页视图模型，用于在目录页面展示商品列表、过滤选项和分页信息
/// </summary>
public class IndexViewModel
{
    /// <summary>
    /// 获取或设置要在页面上显示的商品项集合
    /// </summary>
    public IEnumerable<CatalogItem> CatalogItems { get; set; }

    /// <summary>
    /// 获取或设置品牌过滤下拉列表的选项集合
    /// </summary>
    public IEnumerable<SelectListItem> Brands { get; set; }

    /// <summary>
    /// 获取或设置商品类型过滤下拉列表的选项集合
    /// </summary>
    public IEnumerable<SelectListItem> Types { get; set; }

    /// <summary>
    /// 获取或设置当前应用的品牌过滤器ID值，若未应用过滤则为null
    /// </summary>
    public int? BrandFilterApplied { get; set; }

    /// <summary>
    /// 获取或设置当前应用的商品类型过滤器ID值，若未应用过滤则为null
    /// </summary>
    public int? TypesFilterApplied { get; set; }

    /// <summary>
    /// 获取或设置分页控件所需的分页信息
    /// </summary>
    public PaginationInfo PaginationInfo { get; set; }
}
