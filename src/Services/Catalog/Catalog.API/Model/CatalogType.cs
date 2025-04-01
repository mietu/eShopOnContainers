namespace Microsoft.eShopOnContainers.Services.Catalog.API.Model;

/// <summary>
/// 表示一个商品目录类型，比如电子产品、服装等。
/// </summary>
public class CatalogType
{
    /// <summary>
    /// 唯一标识符，用于区分不同的目录类型。
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 目录类型的名称，如“电子产品”或“服装”。
    /// </summary>
    public string Type { get; set; }
}
