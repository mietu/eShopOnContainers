namespace Microsoft.eShopOnContainers.Services.Catalog.API.Model;

// 商品项类，用于描述目录中的每个商品
public class CatalogItem
{
    // 商品的唯一标识
    public int Id { get; set; }

    // 商品名称
    public string Name { get; set; }

    // 商品描述
    public string Description { get; set; }

    // 商品价格
    public decimal Price { get; set; }

    // 图片文件名
    public string PictureFileName { get; set; }

    // 图片URI路径
    public string PictureUri { get; set; }

    // 商品所属类型的Id
    public int CatalogTypeId { get; set; }

    // 商品所属类型对象
    public CatalogType CatalogType { get; set; }

    // 商品所属品牌的Id
    public int CatalogBrandId { get; set; }

    // 商品所属品牌对象
    public CatalogBrand CatalogBrand { get; set; }

    // 库存中可用的数量
    public int AvailableStock { get; set; }

    // 当库存低于此阈值时触发重新进货
    public int RestockThreshold { get; set; }

    // 仓库中允许的最大库存数量
    public int MaxStockThreshold { get; set; }

    // 是否正在重新订货的标识
    public bool OnReorder { get; set; }

    // 默认构造函数
    public CatalogItem() { }

    /// <summary>
    /// 减少库存数量，同时检查是否低于补货阈值
    /// 如果库存充足，则返回与需求一致的数量；否则返回实际移除的数量
    /// 当库存为0或者请求的数量不合法时，抛出异常
    /// </summary>
    /// <param name="quantityDesired">期望减少的数量</param>
    /// <returns>实际减少的库存数量</returns>
    public int RemoveStock(int quantityDesired)
    {
        // 如果库存为0，则抛出异常
        if (AvailableStock == 0)
        {
            throw new CatalogDomainException($"Empty stock, product item {Name} is sold out");
        }

        // 检查请求的数量必须大于0，否则抛出异常
        if (quantityDesired <= 0)
        {
            throw new CatalogDomainException($"Item units desired should be greater than zero");
        }

        // 计算实际能移除的库存数量（取请求量和可用库存的最小值）
        int removed = Math.Min(quantityDesired, this.AvailableStock);

        // 更新库存
        this.AvailableStock -= removed;

        return removed;
    }

    /// <summary>
    /// 增加库存数量，若增加后超过最大库存阈值，则仅补足至最大库存阈值
    /// 并重设重新订货标识
    /// </summary>
    /// <param name="quantity">要增加的库存数量</param>
    /// <returns>实际增加的库存数量</returns>
    public int AddStock(int quantity)
    {
        int original = this.AvailableStock;

        // 判断如果增加后库存超过最大库存阈值则只补足到最大值
        if ((this.AvailableStock + quantity) > this.MaxStockThreshold)
        {
            // 添加的数量为最大库存与当前库存之差
            this.AvailableStock += (this.MaxStockThreshold - this.AvailableStock);
        }
        else
        {
            // 直接添加提供的数量
            this.AvailableStock += quantity;
        }

        // 重新订货标识重置为false
        this.OnReorder = false;

        return this.AvailableStock - original;
    }
}
