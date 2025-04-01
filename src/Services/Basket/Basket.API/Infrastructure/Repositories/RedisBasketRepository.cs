namespace Microsoft.eShopOnContainers.Services.Basket.API.Infrastructure.Repositories;

/// <summary>
/// 使用 Redis 存储和操作购物篮数据的仓储实现类
/// </summary>
public class RedisBasketRepository : IBasketRepository
{
    // 日志记录器
    private readonly ILogger<RedisBasketRepository> _logger;
    // Redis连接
    private readonly ConnectionMultiplexer _redis;
    // Redis数据操作实例
    private readonly IDatabase _database;

    /// <summary>
    /// 构造函数，初始化 Redis 相关的资源和日志记录器
    /// </summary>
    /// <param name="loggerFactory">日志工厂</param>
    /// <param name="redis">Redis连接</param>
    public RedisBasketRepository(ILoggerFactory loggerFactory, ConnectionMultiplexer redis)
    {
        _logger = loggerFactory.CreateLogger<RedisBasketRepository>();
        _redis = redis;
        _database = redis.GetDatabase();
    }

    /// <summary>
    /// 异步删除指定ID的购物篮数据
    /// </summary>
    /// <param name="id">购物篮标识</param>
    /// <returns>删除是否成功</returns>
    public async Task<bool> DeleteBasketAsync(string id)
    {
        // 使用 Redis 删除键值对
        return await _database.KeyDeleteAsync(id);
    }

    /// <summary>
    /// 获取所有在 Redis 中保存的购物篮用户ID列表
    /// </summary>
    /// <returns>用户ID集合</returns>
    public IEnumerable<string> GetUsers()
    {
        // 获取 Redis 服务器实例
        var server = GetServer();
        // 获取所有键
        var data = server.Keys();
        // 将键转化为字符串集合返回
        return data?.Select(k => k.ToString());
    }

    /// <summary>
    /// 异步获取指定用户的购物篮数据
    /// </summary>
    /// <param name="customerId">顾客的唯一标识符</param>
    /// <returns>顾客购物篮对象</returns>
    public async Task<CustomerBasket> GetBasketAsync(string customerId)
    {
        // 从 Redis 中获取购物篮数据（JSON 格式）
        var data = await _database.StringGetAsync(customerId);
        // 如果数据为空，则返回 null
        if (data.IsNullOrEmpty)
        {
            return null;
        }
        // 将 JSON 数据反序列化成 CustomerBasket 对象
        return JsonSerializer.Deserialize<CustomerBasket>(data, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    /// <summary>
    /// 异步保存或者更新购物篮数据
    /// </summary>
    /// <param name="basket">顾客购物篮对象</param>
    /// <returns>更新后的购物篮对象</returns>
    public async Task<CustomerBasket> UpdateBasketAsync(CustomerBasket basket)
    {
        // 将购物篮对象序列化为 JSON 字符串，并保存到 Redis
        var created = await _database.StringSetAsync(basket.BuyerId, JsonSerializer.Serialize(basket));
        // 如果保存失败，则记录日志并返回 null
        if (!created)
        {
            _logger.LogInformation("保存购物篮数据时出现问题。");
            return null;
        }
        _logger.LogInformation("购物篮数据保存成功。");
        // 返回保存后的购物篮数据
        return await GetBasketAsync(basket.BuyerId);
    }

    /// <summary>
    /// 获取 Redis 服务器实例
    /// </summary>
    /// <returns>IServer 实例</returns>
    private IServer GetServer()
    {
        // 获取所有的 Redis 端点信息，并选择第一个
        var endpoint = _redis.GetEndPoints();
        // 返回第一个端点对应的服务器实例
        return _redis.GetServer(endpoint.First());
    }
}
