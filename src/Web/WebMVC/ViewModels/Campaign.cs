namespace Microsoft.eShopOnContainers.WebMVC.ViewModels;

/// <summary>
/// 表示活动数据的视图模型，用于分页显示活动数据
/// </summary>
public record Campaign
{
    /// <summary>
    /// 当前页索引
    /// </summary>
    public int PageIndex { get; init; }

    /// <summary>
    /// 每页显示的数据条数
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// 总数据条数
    /// </summary>
    public int Count { get; init; }

    /// <summary>
    /// 当前页的活动数据列表
    /// </summary>
    public List<CampaignItem> Data { get; init; }
}
