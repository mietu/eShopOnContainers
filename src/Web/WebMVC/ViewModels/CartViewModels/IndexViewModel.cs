namespace Microsoft.eShopOnContainers.WebMVC.ViewModels.CartViewModels;

/// <summary>
/// 表示购物车组件的视图模型
/// </summary>
public class CartComponentViewModel
{
    /// <summary>
    /// 获取或设置购物车中的商品数量
    /// </summary>
    public int ItemsCount { get; set; }

    /// <summary>
    /// 获取一个CSS类字符串，用于禁用购物车功能
    /// 当购物车为空（ItemsCount为0）时，返回"is-disabled"字符串，否则返回空字符串
    /// </summary>
    public string Disabled => (ItemsCount == 0) ? "is-disabled" : "";
}
