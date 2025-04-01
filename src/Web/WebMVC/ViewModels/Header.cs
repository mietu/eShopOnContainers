namespace Microsoft.eShopOnContainers.WebMVC.ViewModels;

/// <summary>
/// 表示应用程序头部区域的视图模型
/// 作为不可变的记录类型实现，适合在视图间传递头部信息
/// </summary>
public record Header
{
    /// <summary>
    /// 获取或初始化与头部关联的控制器名称
    /// 用于确定头部链接应该导航到哪个控制器
    /// </summary>
    public string Controller { get; init; }

    /// <summary>
    /// 获取或初始化头部要显示的文本内容
    /// 通常用于在UI中显示的标题文本
    /// </summary>
    public string Text { get; init; }
}
