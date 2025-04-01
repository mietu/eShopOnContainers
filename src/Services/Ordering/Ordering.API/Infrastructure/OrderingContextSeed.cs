namespace Microsoft.eShopOnContainers.Services.Ordering.API.Infrastructure;

using Microsoft.eShopOnContainers.Services.Ordering.Domain.AggregatesModel.BuyerAggregate;

/// <summary>
/// 数据库种子类，用于为OrderingContext初始化预定义的数据，例如卡类型与订单状态
/// </summary>
public class OrderingContextSeed
{
    /// <summary>
    /// 种子填充方法，执行数据库迁移并加载初始数据
    /// </summary>
    /// <param name="context">OrderingContext上下文</param>
    /// <param name="env">Web主机环境，用于获取内容根路径</param>
    /// <param name="settings">配置信息</param>
    /// <param name="logger">日志记录器</param>
    public async Task SeedAsync(OrderingContext context, IWebHostEnvironment env, IOptions<OrderingSettings> settings, ILogger<OrderingContextSeed> logger)
    {
        // 创建带重试机制的策略，捕捉SQL异常
        var policy = CreatePolicy(logger, nameof(OrderingContextSeed));

        await policy.ExecuteAsync(async () =>
        {
            // 获取是否使用自定义数据的配置
            var useCustomizationData = settings.Value.UseCustomizationData;
            // 获取当前项目的根路径
            var contentRootPath = env.ContentRootPath;

            // 使用数据库上下文
            using (context)
            {
                // 执行数据库迁移
                context.Database.Migrate();

                // 如果CardTypes表中没有数据，则插入数据
                if (!context.CardTypes.Any())
                {
                    context.CardTypes.AddRange(useCustomizationData
                        ? GetCardTypesFromFile(contentRootPath, logger)  // 从CSV文件中读取数据
                        : GetPredefinedCardTypes());                     // 使用预定义数据

                    await context.SaveChangesAsync();
                }

                // 如果OrderStatus表中没有数据，则插入数据
                if (!context.OrderStatus.Any())
                {
                    context.OrderStatus.AddRange(useCustomizationData
                        ? GetOrderStatusFromFile(contentRootPath, logger) // 从CSV文件中读取数据
                        : GetPredefinedOrderStatus());                    // 使用预定义数据
                }

                await context.SaveChangesAsync();
            }
        });
    }

    /// <summary>
    /// 从CSV文件读取卡类型数据，如果读取失败则返回预定义卡类型
    /// </summary>
    /// <param name="contentRootPath">项目根路径</param>
    /// <param name="log">日志记录器</param>
    /// <returns>卡类型的集合</returns>
    private IEnumerable<CardType> GetCardTypesFromFile(string contentRootPath, ILogger<OrderingContextSeed> log)
    {
        string csvFileCardTypes = Path.Combine(contentRootPath, "Setup", "CardTypes.csv");

        // 如果CSV文件不存在则返回预定义数据
        if (!File.Exists(csvFileCardTypes))
        {
            return GetPredefinedCardTypes();
        }

        string[] csvheaders;
        try
        {
            string[] requiredHeaders = { "CardType" };
            // 从CSV文件读取表头信息
            csvheaders = GetHeaders(requiredHeaders, csvFileCardTypes);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "EXCEPTION ERROR: {Message}", ex.Message);
            return GetPredefinedCardTypes();
        }

        int id = 1;
        // 跳过表头后逐行读取数据，并转换为CardType实例
        return File.ReadAllLines(csvFileCardTypes)
                    .Skip(1) // 跳过表头行
                    .SelectTry(x => CreateCardType(x, ref id))
                    .OnCaughtException(ex => { log.LogError(ex, "EXCEPTION ERROR: {Message}", ex.Message); return null; })
                    .Where(x => x != null);
    }

    /// <summary>
    /// 根据CSV中读取的字符串创建CardType对象
    /// </summary>
    /// <param name="value">CSV记录的字符串</param>
    /// <param name="id">卡类型的编号，递增</param>
    /// <returns>CardType实例</returns>
    private CardType CreateCardType(string value, ref int id)
    {
        if (String.IsNullOrEmpty(value))
        {
            throw new Exception("CardType值为空或null");
        }

        return new CardType(id++, value.Trim('"').Trim());
    }

    /// <summary>
    /// 返回系统内预定义的卡类型数据
    /// </summary>
    private IEnumerable<CardType> GetPredefinedCardTypes()
    {
        return Enumeration.GetAll<CardType>();
    }

    /// <summary>
    /// 从CSV文件中获取订单状态数据，如果失败则返回预定义订单状态
    /// </summary>
    /// <param name="contentRootPath">项目根路径</param>
    /// <param name="log">日志记录器</param>
    /// <returns>订单状态的集合</returns>
    private IEnumerable<OrderStatus> GetOrderStatusFromFile(string contentRootPath, ILogger<OrderingContextSeed> log)
    {
        string csvFileOrderStatus = Path.Combine(contentRootPath, "Setup", "OrderStatus.csv");

        // 如果CSV文件不存在，则返回预定义数据
        if (!File.Exists(csvFileOrderStatus))
        {
            return GetPredefinedOrderStatus();
        }

        string[] csvheaders;
        try
        {
            string[] requiredHeaders = { "OrderStatus" };
            // 从CSV文件读取表头信息
            csvheaders = GetHeaders(requiredHeaders, csvFileOrderStatus);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "EXCEPTION ERROR: {Message}", ex.Message);
            return GetPredefinedOrderStatus();
        }

        int id = 1;
        // 跳过表头后逐行读取记录，并转换为OrderStatus实例
        return File.ReadAllLines(csvFileOrderStatus)
                    .Skip(1) // 跳过表头行
                    .SelectTry(x => CreateOrderStatus(x, ref id))
                    .OnCaughtException(ex => { log.LogError(ex, "EXCEPTION ERROR: {Message}", ex.Message); return null; })
                    .Where(x => x != null);
    }

    /// <summary>
    /// 根据CSV中读取的字符串创建OrderStatus对象，转换名称为小写
    /// </summary>
    /// <param name="value">CSV记录的字符串</param>
    /// <param name="id">订单状态的编号，递增</param>
    /// <returns>OrderStatus实例</returns>
    private OrderStatus CreateOrderStatus(string value, ref int id)
    {
        if (String.IsNullOrEmpty(value))
        {
            throw new Exception("OrderStatus值为空或null");
        }

        return new OrderStatus(id++, value.Trim('"').Trim().ToLowerInvariant());
    }

    /// <summary>
    /// 返回系统预定义的订单状态集合
    /// </summary>
    private IEnumerable<OrderStatus> GetPredefinedOrderStatus()
    {
        return new List<OrderStatus>()
                    {
                        OrderStatus.Submitted,
                        OrderStatus.AwaitingValidation,
                        OrderStatus.StockConfirmed,
                        OrderStatus.Paid,
                        OrderStatus.Shipped,
                        OrderStatus.Cancelled
                    };
    }

    /// <summary>
    /// 读取CSV文件表头，校验所需表头是否存在
    /// </summary>
    /// <param name="requiredHeaders">必须存在的表头数组</param>
    /// <param name="csvfile">CSV文件路径</param>
    /// <returns>CSV文件表头的字符串数组</returns>
    private string[] GetHeaders(string[] requiredHeaders, string csvfile)
    {
        // 仅读取第一行，并转换为小写，再根据逗号分割
        string[] csvheaders = File.ReadLines(csvfile).First().ToLowerInvariant().Split(',');

        // 如果读取的表头数量与预期不匹配，则抛出异常
        if (csvheaders.Count() != requiredHeaders.Count())
        {
            throw new Exception($"requiredHeader count '{requiredHeaders.Count()}' is different then read header '{csvheaders.Count()}'");
        }

        // 校验每个必须的表头是否存在
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
    /// 创建一个Polly异步重试策略，用于处理SqlException异常
    /// </summary>
    /// <param name="logger">日志记录器</param>
    /// <param name="prefix">日志前缀</param>
    /// <param name="retries">重试次数</param>
    /// <returns>AsyncRetryPolicy实例</returns>
    private AsyncRetryPolicy CreatePolicy(ILogger<OrderingContextSeed> logger, string prefix, int retries = 3)
    {
        return Policy.Handle<SqlException>().
            WaitAndRetryAsync(
                retryCount: retries,
                sleepDurationProvider: retry => TimeSpan.FromSeconds(5),
                onRetry: (exception, timeSpan, retry, ctx) =>
                {
                    // 每次重试时记录警告日志
                    logger.LogWarning(exception, "[{prefix}] Exception {ExceptionType} with message {Message} detected on attempt {retry} of {retries}",
                        prefix, exception.GetType().Name, exception.Message, retry, retries);
                }
            );
    }
}
