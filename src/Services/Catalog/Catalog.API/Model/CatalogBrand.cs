namespace Microsoft.eShopOnContainers.Services.Catalog.API.Model;

/// <summary>
/// CatalogBrand 类表示产品目录中的品牌。
/// </summary>
public class CatalogBrand
{
    // 品牌的唯一标识符
    public int Id { get; set; }

    // 品牌名称
    public string Brand { get; set; }
}
