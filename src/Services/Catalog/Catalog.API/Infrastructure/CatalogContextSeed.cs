namespace Microsoft.eShopOnContainers.Services.Catalog.API.Infrastructure;

/// <summary>
/// 该类用于对CatalogContext进行数据种子初始化，比如导入品牌、类型以及物品记录。
/// </summary>
public class CatalogContextSeed
{
    /// <summary>
    /// 异步执行种子数据初始化。依赖于IWebHostEnvironment、CatalogSettings配置和日志记录器。
    /// </summary>
    /// <param name="context">数据库上下文CatalogContext</param>
    /// <param name="env">主机环境信息</param>
    /// <param name="settings">配置信息</param>
    /// <param name="logger">日志记录器</param>
    public async Task SeedAsync(CatalogContext context, IWebHostEnvironment env, IOptions<CatalogSettings> settings, ILogger<CatalogContextSeed> logger)
    {
        // 创建重试策略，当出现SqlException时重试3次，每次间隔5秒
        var policy = CreatePolicy(logger, nameof(CatalogContextSeed));

        await policy.ExecuteAsync(async () =>
        {
            var useCustomizationData = settings.Value.UseCustomizationData; // 是否使用自定义数据
            var contentRootPath = env.ContentRootPath; // 应用内容根目录
            var picturePath = env.WebRootPath;       // 图片的web目录路径

            // 如果数据库中没有CatalogBrands则导入数据
            if (!context.CatalogBrands.Any())
            {
                await context.CatalogBrands.AddRangeAsync(useCustomizationData
                    ? GetCatalogBrandsFromFile(contentRootPath, logger)
                    : GetPreconfiguredCatalogBrands());

                await context.SaveChangesAsync();
            }

            // 如果数据库中没有CatalogTypes则导入数据
            if (!context.CatalogTypes.Any())
            {
                await context.CatalogTypes.AddRangeAsync(useCustomizationData
                    ? GetCatalogTypesFromFile(contentRootPath, logger)
                    : GetPreconfiguredCatalogTypes());

                await context.SaveChangesAsync();
            }

            // 如果数据库中没有CatalogItems则导入数据
            if (!context.CatalogItems.Any())
            {
                await context.CatalogItems.AddRangeAsync(useCustomizationData
                    ? GetCatalogItemsFromFile(contentRootPath, context, logger)
                    : GetPreconfiguredItems());

                await context.SaveChangesAsync();

                // 导入CatalogItem图片到WebRoot目录
                GetCatalogItemPictures(contentRootPath, picturePath);
            }
        });
    }

    /// <summary>
    /// 从CSV文件中获取Catalog品牌数据，如果文件不存在或异常则返回预配置的数据。
    /// </summary>
    /// <param name="contentRootPath">应用根目录</param>
    /// <param name="logger">日志记录器</param>
    /// <returns>CatalogBrand集合</returns>
    private IEnumerable<CatalogBrand> GetCatalogBrandsFromFile(string contentRootPath, ILogger<CatalogContextSeed> logger)
    {
        string csvFileCatalogBrands = Path.Combine(contentRootPath, "Setup", "CatalogBrands.csv");

        if (!File.Exists(csvFileCatalogBrands))
        {
            return GetPreconfiguredCatalogBrands();
        }

        string[] csvheaders;
        try
        {
            string[] requiredHeaders = { "catalogbrand" };
            csvheaders = GetHeaders(csvFileCatalogBrands, requiredHeaders);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "EXCEPTION ERROR: {Message}", ex.Message);
            return GetPreconfiguredCatalogBrands();
        }

        // 跳过标题行，并尝试解析每一行数据
        return File.ReadAllLines(csvFileCatalogBrands)
                                .Skip(1) // 跳过标题行
                                .SelectTry(x => CreateCatalogBrand(x))
                                .OnCaughtException(ex =>
                                {
                                    logger.LogError(ex, "EXCEPTION ERROR: {Message}", ex.Message);
                                    return null;
                                })
                                .Where(x => x != null);
    }

    /// <summary>
    /// 解析单行数据，生成一个CatalogBrand对象
    /// </summary>
    /// <param name="brand">CSV中的一行品牌数据</param>
    /// <returns>CatalogBrand对象</returns>
    private CatalogBrand CreateCatalogBrand(string brand)
    {
        // 去除两边多余的引号与空格
        brand = brand.Trim('"').Trim();

        if (String.IsNullOrEmpty(brand))
        {
            throw new Exception("catalog Brand Name is empty");
        }

        return new CatalogBrand
        {
            Brand = brand,
        };
    }

    /// <summary>
    /// 返回预配置的Catalog品牌集合
    /// </summary>
    /// <returns>CatalogBrand集合</returns>
    private IEnumerable<CatalogBrand> GetPreconfiguredCatalogBrands()
    {
        return new List<CatalogBrand>()
            {
                new() { Brand = "Azure"},
                new() { Brand = ".NET" },
                new() { Brand = "Visual Studio" },
                new() { Brand = "SQL Server" },
                new() { Brand = "Other" }
            };
    }

    /// <summary>
    /// 从CSV文件中获取Catalog类型数据，如果文件不存在或读取异常则返回预配置的数据
    /// </summary>
    /// <param name="contentRootPath">应用根目录</param>
    /// <param name="logger">日志记录器</param>
    /// <returns>CatalogType集合</returns>
    private IEnumerable<CatalogType> GetCatalogTypesFromFile(string contentRootPath, ILogger<CatalogContextSeed> logger)
    {
        string csvFileCatalogTypes = Path.Combine(contentRootPath, "Setup", "CatalogTypes.csv");

        if (!File.Exists(csvFileCatalogTypes))
        {
            return GetPreconfiguredCatalogTypes();
        }

        string[] csvheaders;
        try
        {
            string[] requiredHeaders = { "catalogtype" };
            csvheaders = GetHeaders(csvFileCatalogTypes, requiredHeaders);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "EXCEPTION ERROR: {Message}", ex.Message);
            return GetPreconfiguredCatalogTypes();
        }

        return File.ReadAllLines(csvFileCatalogTypes)
                                .Skip(1) // 跳过标题行
                                .SelectTry(x => CreateCatalogType(x))
                                .OnCaughtException(ex =>
                                {
                                    logger.LogError(ex, "EXCEPTION ERROR: {Message}", ex.Message);
                                    return null;
                                })
                                .Where(x => x != null);
    }

    /// <summary>
    /// 解析单行数据，生成一个CatalogType对象
    /// </summary>
    /// <param name="type">CSV中的一行类型数据</param>
    /// <returns>CatalogType对象</returns>
    private CatalogType CreateCatalogType(string type)
    {
        type = type.Trim('"').Trim();

        if (String.IsNullOrEmpty(type))
        {
            throw new Exception("catalog Type Name is empty");
        }

        return new CatalogType
        {
            Type = type,
        };
    }

    /// <summary>
    /// 返回预配置的Catalog类型集合
    /// </summary>
    /// <returns>CatalogType集合</returns>
    private IEnumerable<CatalogType> GetPreconfiguredCatalogTypes()
    {
        return new List<CatalogType>()
            {
                new() { Type = "Mug"},
                new() { Type = "T-Shirt" },
                new() { Type = "Sheet" },
                new() { Type = "USB Memory Stick" }
            };
    }

    /// <summary>
    /// 从CSV文件中获取Catalog物品数据。如果文件不存在或读取异常则返回预配置的物品数据。
    /// </summary>
    /// <param name="contentRootPath">应用根目录</param>
    /// <param name="context">当前CatalogContext上下文</param>
    /// <param name="logger">日志记录器</param>
    /// <returns>CatalogItem集合</returns>
    private IEnumerable<CatalogItem> GetCatalogItemsFromFile(string contentRootPath, CatalogContext context, ILogger<CatalogContextSeed> logger)
    {
        string csvFileCatalogItems = Path.Combine(contentRootPath, "Setup", "CatalogItems.csv");

        if (!File.Exists(csvFileCatalogItems))
        {
            return GetPreconfiguredItems();
        }

        string[] csvheaders;
        try
        {
            // 定义必须的和可选的CSV文件表头
            string[] requiredHeaders = { "catalogtypename", "catalogbrandname", "description", "name", "price", "picturefilename" };
            string[] optionalheaders = { "availablestock", "restockthreshold", "maxstockthreshold", "onreorder" };
            csvheaders = GetHeaders(csvFileCatalogItems, requiredHeaders, optionalheaders);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "EXCEPTION ERROR: {Message}", ex.Message);
            return GetPreconfiguredItems();
        }

        // 利用上下文中已有的类型和品牌数据构造字典
        var catalogTypeIdLookup = context.CatalogTypes.ToDictionary(ct => ct.Type, ct => ct.Id);
        var catalogBrandIdLookup = context.CatalogBrands.ToDictionary(ct => ct.Brand, ct => ct.Id);

        return File.ReadAllLines(csvFileCatalogItems)
                    .Skip(1) // 跳过标题行
                    .Select(row => Regex.Split(row, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)"))
                    .SelectTry(column => CreateCatalogItem(column, csvheaders, catalogTypeIdLookup, catalogBrandIdLookup))
                    .OnCaughtException(ex =>
                    {
                        logger.LogError(ex, "EXCEPTION ERROR: {Message}", ex.Message);
                        return null;
                    })
                    .Where(x => x != null);
    }

    /// <summary>
    /// 解析CSV行数据，生成一个CatalogItem对象，同时根据字典查找外键关系。
    /// </summary>
    /// <param name="column">CSV拆分后的字段数组</param>
    /// <param name="headers">CSV表头数组</param>
    /// <param name="catalogTypeIdLookup">Catalog类型名称到Id的映射</param>
    /// <param name="catalogBrandIdLookup">Catalog品牌名称到Id的映射</param>
    /// <returns>CatalogItem对象</returns>
    private CatalogItem CreateCatalogItem(string[] column, string[] headers, Dictionary<String, int> catalogTypeIdLookup, Dictionary<String, int> catalogBrandIdLookup)
    {
        if (column.Count() != headers.Count())
        {
            throw new Exception($"column count '{column.Count()}' not the same as headers count'{headers.Count()}'");
        }

        // 获取Catalog类型名称，并验证在字典中存在
        string catalogTypeName = column[Array.IndexOf(headers, "catalogtypename")].Trim('"').Trim();
        if (!catalogTypeIdLookup.ContainsKey(catalogTypeName))
        {
            throw new Exception($"type={catalogTypeName} does not exist in catalogTypes");
        }

        // 获取Catalog品牌名称，并验证在字典中存在
        string catalogBrandName = column[Array.IndexOf(headers, "catalogbrandname")].Trim('"').Trim();
        if (!catalogBrandIdLookup.ContainsKey(catalogBrandName))
        {
            throw new Exception($"type={catalogBrandName} does not exist in catalogTypes");
        }

        // 解析价格字段
        string priceString = column[Array.IndexOf(headers, "price")].Trim('"').Trim();
        if (!Decimal.TryParse(priceString, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out Decimal price))
        {
            throw new Exception($"price={priceString}is not a valid decimal number");
        }

        // 构建CatalogItem对象
        var catalogItem = new CatalogItem()
        {
            CatalogTypeId = catalogTypeIdLookup[catalogTypeName],
            CatalogBrandId = catalogBrandIdLookup[catalogBrandName],
            Description = column[Array.IndexOf(headers, "description")].Trim('"').Trim(),
            Name = column[Array.IndexOf(headers, "name")].Trim('"').Trim(),
            Price = price,
            PictureFileName = column[Array.IndexOf(headers, "picturefilename")].Trim('"').Trim(),
        };

        // 处理可选字段：availablestock
        int availableStockIndex = Array.IndexOf(headers, "availablestock");
        if (availableStockIndex != -1)
        {
            string availableStockString = column[availableStockIndex].Trim('"').Trim();
            if (!String.IsNullOrEmpty(availableStockString))
            {
                if (int.TryParse(availableStockString, out int availableStock))
                {
                    catalogItem.AvailableStock = availableStock;
                }
                else
                {
                    throw new Exception($"availableStock={availableStockString} is not a valid integer");
                }
            }
        }

        // 处理可选字段：restockthreshold
        int restockThresholdIndex = Array.IndexOf(headers, "restockthreshold");
        if (restockThresholdIndex != -1)
        {
            string restockThresholdString = column[restockThresholdIndex].Trim('"').Trim();
            if (!String.IsNullOrEmpty(restockThresholdString))
            {
                if (int.TryParse(restockThresholdString, out int restockThreshold))
                {
                    catalogItem.RestockThreshold = restockThreshold;
                }
                else
                {
                    throw new Exception($"restockThreshold={restockThresholdString} is not a valid integer");
                }
            }
        }

        // 处理可选字段：maxstockthreshold
        int maxStockThresholdIndex = Array.IndexOf(headers, "maxstockthreshold");
        if (maxStockThresholdIndex != -1)
        {
            string maxStockThresholdString = column[maxStockThresholdIndex].Trim('"').Trim();
            if (!String.IsNullOrEmpty(maxStockThresholdString))
            {
                if (int.TryParse(maxStockThresholdString, out int maxStockThreshold))
                {
                    catalogItem.MaxStockThreshold = maxStockThreshold;
                }
                else
                {
                    throw new Exception($"maxStockThreshold={maxStockThresholdString} is not a valid integer");
                }
            }
        }

        // 处理可选字段：onreorder
        int onReorderIndex = Array.IndexOf(headers, "onreorder");
        if (onReorderIndex != -1)
        {
            string onReorderString = column[onReorderIndex].Trim('"').Trim();
            if (!String.IsNullOrEmpty(onReorderString))
            {
                if (bool.TryParse(onReorderString, out bool onReorder))
                {
                    catalogItem.OnReorder = onReorder;
                }
                else
                {
                    throw new Exception($"onReorder={onReorderString} is not a valid boolean");
                }
            }
        }

        return catalogItem;
    }

    /// <summary>
    /// 返回预配置的Catalog物品数据集合
    /// </summary>
    /// <returns>CatalogItem集合</returns>
    private IEnumerable<CatalogItem> GetPreconfiguredItems()
    {
        return new List<CatalogItem>()
            {
                new() { CatalogTypeId = 2, CatalogBrandId = 2, AvailableStock = 100, Description = ".NET Bot Black Hoodie", Name = ".NET Bot Black Hoodie", Price = 19.5M, PictureFileName = "1.png" },
                new() { CatalogTypeId = 1, CatalogBrandId = 2, AvailableStock = 100, Description = ".NET Black & White Mug", Name = ".NET Black & White Mug", Price= 8.50M, PictureFileName = "2.png" },
                new() { CatalogTypeId = 2, CatalogBrandId = 5, AvailableStock = 100, Description = "Prism White T-Shirt", Name = "Prism White T-Shirt", Price = 12, PictureFileName = "3.png" },
                new() { CatalogTypeId = 2, CatalogBrandId = 2, AvailableStock = 100, Description = ".NET Foundation T-shirt", Name = ".NET Foundation T-shirt", Price = 12, PictureFileName = "4.png" },
                new() { CatalogTypeId = 3, CatalogBrandId = 5, AvailableStock = 100, Description = "Roslyn Red Sheet", Name = "Roslyn Red Sheet", Price = 8.5M, PictureFileName = "5.png" },
                new() { CatalogTypeId = 2, CatalogBrandId = 2, AvailableStock = 100, Description = ".NET Blue Hoodie", Name = ".NET Blue Hoodie", Price = 12, PictureFileName = "6.png" },
                new() { CatalogTypeId = 2, CatalogBrandId = 5, AvailableStock = 100, Description = "Roslyn Red T-Shirt", Name = "Roslyn Red T-Shirt", Price = 12, PictureFileName = "7.png" },
                new() { CatalogTypeId = 2, CatalogBrandId = 5, AvailableStock = 100, Description = "Kudu Purple Hoodie", Name = "Kudu Purple Hoodie", Price = 8.5M, PictureFileName = "8.png" },
                new() { CatalogTypeId = 1, CatalogBrandId = 5, AvailableStock = 100, Description = "Cup<T> White Mug", Name = "Cup<T> White Mug", Price = 12, PictureFileName = "9.png" },
                new() { CatalogTypeId = 3, CatalogBrandId = 2, AvailableStock = 100, Description = ".NET Foundation Sheet", Name = ".NET Foundation Sheet", Price = 12, PictureFileName = "10.png" },
                new() { CatalogTypeId = 3, CatalogBrandId = 2, AvailableStock = 100, Description = "Cup<T> Sheet", Name = "Cup<T> Sheet", Price = 8.5M, PictureFileName = "11.png" },
                new() { CatalogTypeId = 2, CatalogBrandId = 5, AvailableStock = 100, Description = "Prism White TShirt", Name = "Prism White TShirt", Price = 12, PictureFileName = "12.png" },
            };
    }

    /// <summary>
    /// 读取CSV文件的表头，并验证必需字段与可选字段。若不满足条件则抛出异常。
    /// </summary>
    /// <param name="csvfile">CSV文件路径</param>
    /// <param name="requiredHeaders">必需的CSV表头</param>
    /// <param name="optionalHeaders">可选的CSV表头</param>
    /// <returns>CSV文件中的表头数组</returns>
    private string[] GetHeaders(string csvfile, string[] requiredHeaders, string[] optionalHeaders = null)
    {
        // 获取CSV文件第一行，并转换为小写
        string[] csvheaders = File.ReadLines(csvfile).First().ToLowerInvariant().Split(',');

        if (csvheaders.Count() < requiredHeaders.Count())
        {
            throw new Exception($"requiredHeader count '{requiredHeaders.Count()}' is bigger then csv header count '{csvheaders.Count()}' ");
        }

        if (optionalHeaders != null)
        {
            if (csvheaders.Count() > (requiredHeaders.Count() + optionalHeaders.Count()))
            {
                throw new Exception($"csv header count '{csvheaders.Count()}'  is larger then required '{requiredHeaders.Count()}' and optional '{optionalHeaders.Count()}' headers count");
            }
        }

        foreach (var requiredHeader in requiredHeaders)
        {
            if (!csvheaders.Contains(requiredHeader))
            {
                throw new Exception($"does not contain required header '{requiredHeader}'");
            }
        }

        return csvheaders;
    }

    /// <summary>
    /// 将Catalog物品的图片删除后，从ZIP文件中提取图片到WebRoot路径中
    /// </summary>
    /// <param name="contentRootPath">应用根目录</param>
    /// <param name="picturePath">图片所在的Web目录路径</param>
    private void GetCatalogItemPictures(string contentRootPath, string picturePath)
    {
        if (picturePath != null)
        {
            // 删除Web目录中现有的所有文件
            DirectoryInfo directory = new DirectoryInfo(picturePath);
            foreach (FileInfo file in directory.GetFiles())
            {
                file.Delete();
            }

            string zipFileCatalogItemPictures = Path.Combine(contentRootPath, "Setup", "CatalogItems.zip");
            // 解压ZIP文件到Web目录
            ZipFile.ExtractToDirectory(zipFileCatalogItemPictures, picturePath);
        }
    }

    /// <summary>
    /// 创建一个重试策略，当执行过程中捕获SqlException时，将重试指定次数。
    /// </summary>
    /// <param name="logger">日志记录器</param>
    /// <param name="prefix">日志中使用的前缀</param>
    /// <param name="retries">重试次数（默认为3）</param>
    /// <returns>AsyncRetryPolicy实例</returns>
    private AsyncRetryPolicy CreatePolicy(ILogger<CatalogContextSeed> logger, string prefix, int retries = 3)
    {
        return Policy.Handle<SqlException>().
            WaitAndRetryAsync(
                retryCount: retries,
                sleepDurationProvider: retry => TimeSpan.FromSeconds(5),
                onRetry: (exception, timeSpan, retry, ctx) =>
                {
                    logger.LogWarning(exception, "[{prefix}] Exception {ExceptionType} with message {Message} detected on attempt {retry} of {retries}", prefix, exception.GetType().Name, exception.Message, retry, retries);
                }
            );
    }
}
