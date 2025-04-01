namespace Microsoft.eShopOnContainers.WebMVC.ViewModels.Pagination;

/// <summary>
/// 表示分页信息的视图模型类
/// </summary>
public class PaginationInfo
{
    /// <summary>
    /// 获取或设置数据集中的总项目数
    /// </summary>
    public int TotalItems { get; set; }

    /// <summary>
    /// 获取或设置每页显示的项目数
    /// </summary>
    public int ItemsPerPage { get; set; }

    /// <summary>
    /// 获取或设置当前页码
    /// </summary>
    public int ActualPage { get; set; }

    /// <summary>
    /// 获取或设置总页数
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// 获取或设置前一页的URL或标识符
    /// </summary>
    public string Previous { get; set; }

    /// <summary>
    /// 获取或设置下一页的URL或标识符
    /// </summary>
    public string Next { get; set; }
}
