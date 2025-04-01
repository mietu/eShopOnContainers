namespace Microsoft.eShopOnContainers.Services.Catalog.API.Model;

/// <summary>
/// 为 CatalogItem 对象添加扩展方法
/// </summary>
public static class CatalogItemExtensions
{
    /// <summary>
    /// 根据给定的图片基础 URL 与是否启用了 Azure 存储的配置，填充 CatalogItem 的 PictureUri 属性
    /// </summary>
    /// <param name="item">待处理的 CatalogItem 对象</param>
    /// <param name="picBaseUrl">图片基础 URL</param>
    /// <param name="azureStorageEnabled">
    /// 布尔值，指示是否启用了 Azure 存储。如果为 true，则使用拼接方式生成 URL；
    /// 如果为 false，则使用替换方式将 [0] 替换成 item 的 Id 值
    /// </param>
    public static void FillProductUrl(this CatalogItem item, string picBaseUrl, bool azureStorageEnabled)
    {
        if (item != null)
        {
            // 判断是否启用 Azure 存储
            // 如果启用，则直接拼接图像基础 URL 和图片文件名生成完整 URL
            // 否则，将图像基础 URL 中的 "[0]" 替换为 item 的 Id 值
            item.PictureUri = azureStorageEnabled
                ? picBaseUrl + item.PictureFileName
                : picBaseUrl.Replace("[0]", item.Id.ToString());
        }
    }
}
