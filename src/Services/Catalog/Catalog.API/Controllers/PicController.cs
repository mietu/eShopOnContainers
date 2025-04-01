// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860
namespace Microsoft.eShopOnContainers.Services.Catalog.API.Controllers;

[ApiController]
public class PicController : ControllerBase
{
    // 用于获取Web根目录，帮助定位图片文件位置
    private readonly IWebHostEnvironment _env;
    // 数据库上下文，用于访问CatalogItems等数据
    private readonly CatalogContext _catalogContext;

    // 构造函数，通过依赖注入获取 IWebHostEnvironment 和 CatalogContext 实例
    public PicController(IWebHostEnvironment env,
        CatalogContext catalogContext)
    {
        _env = env;
        _catalogContext = catalogContext;
    }

    // HTTP GET 请求，用于获取指定产品ID的图片
    // 访问路径: api/v1/catalog/items/{catalogItemId:int}/pic
    [HttpGet]
    [Route("api/v1/catalog/items/{catalogItemId:int}/pic")]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    // 当catalogItemId无效时返回坏请求，当找不到产品时返回NotFound
    public async Task<ActionResult> GetImageAsync(int catalogItemId)
    {
        // 检查产品ID是否无效
        if (catalogItemId <= 0)
        {
            return BadRequest();
        }

        // 根据catalogItemId查询产品
        var item = await _catalogContext.CatalogItems
            .SingleOrDefaultAsync(ci => ci.Id == catalogItemId);

        if (item != null)
        {
            // 获取Web根目录路径
            var webRoot = _env.WebRootPath;
            // 拼接图片的完整路径
            var path = Path.Combine(webRoot, item.PictureFileName);

            // 获取文件后缀名（扩展名，例如 .png）
            string imageFileExtension = Path.GetExtension(item.PictureFileName);
            // 根据扩展名获取对应的MIME类型
            string mimetype = GetImageMimeTypeFromImageFileExtension(imageFileExtension);

            // 读取图片文件数据到缓冲区
            var buffer = await System.IO.File.ReadAllBytesAsync(path);

            // 返回图片文件的内容与MIME类型
            return File(buffer, mimetype);
        }

        // 如果找不到产品，则返回NotFound
        return NotFound();
    }

    // 根据图片扩展名获取对应的MIME类型
    private string GetImageMimeTypeFromImageFileExtension(string extension)
    {
        // 使用switch表达式匹配常见的图片扩展名
        string mimetype = extension switch
        {
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".bmp" => "image/bmp",
            ".tiff" => "image/tiff",
            ".wmf" => "image/wmf",
            ".jp2" => "image/jp2",
            ".svg" => "image/svg+xml",
            _ => "application/octet-stream", // 默认类型：二进制流
        };
        return mimetype;
    }
}
