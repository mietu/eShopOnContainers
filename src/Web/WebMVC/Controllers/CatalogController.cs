namespace Microsoft.eShopOnContainers.WebMVC.Controllers;

/// <summary>
/// 负责处理商品目录相关的HTTP请求
/// </summary>
public class CatalogController : Controller
{
    private readonly ICatalogService _catalogSvc;

    /// <summary>
    /// 初始化控制器实例，通过依赖注入获取目录服务
    /// </summary>
    /// <param name="catalogSvc">目录服务接口，用于获取商品数据</param>
    public CatalogController(ICatalogService catalogSvc) =>
        _catalogSvc = catalogSvc;

    /// <summary>
    /// 处理目录主页的GET请求，展示商品列表并支持分页和过滤
    /// </summary>
    /// <param name="BrandFilterApplied">应用的品牌过滤器ID</param>
    /// <param name="TypesFilterApplied">应用的类型过滤器ID</param>
    /// <param name="page">当前请求的页码，从0开始</param>
    /// <param name="errorMsg">可选的错误消息，通常用于显示购物车相关错误</param>
    /// <returns>包含商品目录视图模型的视图</returns>
    public async Task<IActionResult> Index(int? BrandFilterApplied, int? TypesFilterApplied, int? page, [FromQuery] string errorMsg)
    {
        // 设置每页显示的商品数量
        var itemsPage = 9;

        // 从服务获取商品目录数据，应用筛选条件
        var catalog = await _catalogSvc.GetCatalogItems(page ?? 0, itemsPage, BrandFilterApplied, TypesFilterApplied);

        // 构建视图模型，包含商品数据、筛选选项和分页信息
        var vm = new IndexViewModel()
        {
            // 设置目录商品列表
            CatalogItems = catalog.Data,
            // 获取所有可用品牌作为筛选选项
            Brands = await _catalogSvc.GetBrands(),
            // 获取所有可用类型作为筛选选项
            Types = await _catalogSvc.GetTypes(),
            // 设置当前应用的品牌筛选值（如无则为0）
            BrandFilterApplied = BrandFilterApplied ?? 0,
            // 设置当前应用的类型筛选值（如无则为0）
            TypesFilterApplied = TypesFilterApplied ?? 0,
            // 配置分页信息
            PaginationInfo = new PaginationInfo()
            {
                // 当前页码（如无则为首页）
                ActualPage = page ?? 0,
                // 当前页面的实际商品数量
                ItemsPerPage = catalog.Data.Count,
                // 满足筛选条件的总商品数
                TotalItems = catalog.Count,
                // 计算总页数（向上取整）
                TotalPages = (int)Math.Ceiling(((decimal)catalog.Count / itemsPage))
            }
        };

        // 设置"下一页"按钮状态 - 如果当前是最后一页则禁用
        vm.PaginationInfo.Next = (vm.PaginationInfo.ActualPage == vm.PaginationInfo.TotalPages - 1) ? "is-disabled" : "";
        // 设置"上一页"按钮状态 - 如果当前是第一页则禁用
        vm.PaginationInfo.Previous = (vm.PaginationInfo.ActualPage == 0) ? "is-disabled" : "";

        // 通过ViewBag传递购物车错误消息（如果有）
        ViewBag.BasketInoperativeMsg = errorMsg;

        // 返回视图，传入视图模型
        return View(vm);
    }
}
