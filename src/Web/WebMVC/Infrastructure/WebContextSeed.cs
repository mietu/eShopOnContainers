namespace WebMVC.Infrastructure;
using Serilog;

/// <summary>
/// 负责在应用程序启动时初始化网站数据和静态资源的类
/// </summary>
public class WebContextSeed
{
    /// <summary>
    /// 根据应用程序配置初始化静态资源（如自定义图片和CSS）
    /// </summary>
    /// <param name="applicationBuilder">应用程序构建器，用于访问服务</param>
    /// <param name="env">Web主机环境，提供内容根路径和Web根路径</param>
    public static void Seed(IApplicationBuilder applicationBuilder, IWebHostEnvironment env)
    {
        // 获取应用程序的日志记录器
        var log = Serilog.Log.Logger;

        // 从依赖注入容器获取应用程序设置
        var settings = (AppSettings)applicationBuilder
            .ApplicationServices.GetRequiredService<IOptions<AppSettings>>().Value;

        // 确定是否应使用自定义数据
        var useCustomizationData = settings.UseCustomizationData;
        var contentRootPath = env.ContentRootPath; // 应用程序内容根目录
        var webroot = env.WebRootPath;            // 公共Web根目录（wwwroot）

        // 如果启用了自定义设置，复制自定义资源
        if (useCustomizationData)
        {
            GetPreconfiguredImages(contentRootPath, webroot, log);
            GetPreconfiguredCSS(contentRootPath, webroot, log);
        }
    }

    /// <summary>
    /// 将自定义CSS文件从Setup目录复制到网站的css目录
    /// </summary>
    /// <param name="contentRootPath">应用程序内容根目录</param>
    /// <param name="webroot">Web根目录</param>
    /// <param name="log">日志记录器</param>
    static void GetPreconfiguredCSS(string contentRootPath, string webroot, ILogger log)
    {
        try
        {
            // 查找Setup目录中的override.css文件
            string overrideCssFile = Path.Combine(contentRootPath, "Setup", "override.css");
            if (!File.Exists(overrideCssFile))
            {
                log.Error("Override css file '{FileName}' does not exists.", overrideCssFile);
                return;
            }

            // 将CSS文件复制到网站的css目录，如果已存在则覆盖
            string destinationFilename = Path.Combine(webroot, "css", "override.css");
            File.Copy(overrideCssFile, destinationFilename, true);
        }
        catch (Exception ex)
        {
            log.Error(ex, "EXCEPTION ERROR: {Message}", ex.Message);
        }
    }

    /// <summary>
    /// 从Setup目录中的ZIP文件提取图片到网站的images目录
    /// </summary>
    /// <param name="contentRootPath">应用程序内容根目录</param>
    /// <param name="webroot">Web根目录</param>
    /// <param name="log">日志记录器</param>
    static void GetPreconfiguredImages(string contentRootPath, string webroot, ILogger log)
    {
        try
        {
            // 查找Setup目录中的images.zip文件
            string imagesZipFile = Path.Combine(contentRootPath, "Setup", "images.zip");
            if (!File.Exists(imagesZipFile))
            {
                log.Error("Zip file '{ZipFileName}' does not exists.", imagesZipFile);
                return;
            }

            // 获取网站images目录中已有的图片文件列表
            string imagePath = Path.Combine(webroot, "images");
            string[] imageFiles = Directory.GetFiles(imagePath).Select(file => Path.GetFileName(file)).ToArray();

            // 打开ZIP文件并提取内容
            using ZipArchive zip = ZipFile.Open(imagesZipFile, ZipArchiveMode.Read);
            foreach (ZipArchiveEntry entry in zip.Entries)
            {
                // 只提取已经在images目录中存在的文件（覆盖现有文件）
                if (imageFiles.Contains(entry.Name))
                {
                    string destinationFilename = Path.Combine(imagePath, entry.Name);
                    if (File.Exists(destinationFilename))
                    {
                        File.Delete(destinationFilename);
                    }
                    entry.ExtractToFile(destinationFilename);
                }
                else
                {
                    // 记录跳过的文件
                    log.Warning("Skipped file '{FileName}' in zipfile '{ZipFileName}'", entry.Name, imagesZipFile);
                }
            }
        }
        catch (Exception ex)
        {
            log.Error(ex, "EXCEPTION ERROR: {Message}", ex.Message);
        }
    }
}
