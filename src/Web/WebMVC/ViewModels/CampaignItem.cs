namespace Microsoft.eShopOnContainers.WebMVC.ViewModels;

/// <summary>
/// 表示营销活动项的视图模型
/// 使用C# record类型实现，提供值相等性比较和不可变特性
/// </summary>
public record CampaignItem
{
    /// <summary>
    /// 获取或初始化营销活动的唯一标识符
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// 获取或初始化营销活动的名称
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// 获取或初始化营销活动的描述
    /// </summary>
    public string Description { get; init; }

    /// <summary>
    /// 获取或初始化营销活动的开始日期时间
    /// </summary>
    public DateTime From { get; init; }

    /// <summary>
    /// 获取或初始化营销活动的结束日期时间
    /// </summary>
    public DateTime To { get; init; }

    /// <summary>
    /// 获取或初始化营销活动图片的URI
    /// </summary>
    public string PictureUri { get; init; }

    /// <summary>
    /// 获取或初始化营销活动详情页面的URI
    /// </summary>
    public string DetailsUri { get; init; }
}
