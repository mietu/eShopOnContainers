namespace Microsoft.eShopOnContainers.Services.Catalog.API.ViewModel;

// 泛型分页数据视图模型，用于封装分页后的数据集合
public class PaginatedItemsViewModel<TEntity> where TEntity : class
{
    // 当前页的页码索引（从0开始或者1，根据具体约定）
    public int PageIndex { get; private set; }

    // 每页显示的数据条数
    public int PageSize { get; private set; }

    // 数据集中所有项目的总数量
    public long Count { get; private set; }

    // 当前页包含的数据集合
    public IEnumerable<TEntity> Data { get; private set; }

    // 构造函数，初始化分页视图模型的所有属性
    public PaginatedItemsViewModel(int pageIndex, int pageSize, long count, IEnumerable<TEntity> data)
    {
        PageIndex = pageIndex; // 设置当前页码索引
        PageSize = pageSize;   // 设置每页显示的数据条数
        Count = count;         // 设置数据总数
        Data = data;           // 设置当前页的数据集合
    }
}
