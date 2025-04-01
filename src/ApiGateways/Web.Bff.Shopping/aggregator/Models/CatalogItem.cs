namespace Microsoft.eShopOnContainers.Web.Shopping.HttpAggregator.Models;

public class CatalogItem
{
    // 属性：产品的唯一标识符
    public int Id { get; set; }

    // 属性：产品的名称
    public string Name { get; set; }

    // 属性：产品的价格
    public decimal Price { get; set; }

    // 属性：产品图片的 URI 地址
    public string PictureUri { get; set; }
}


