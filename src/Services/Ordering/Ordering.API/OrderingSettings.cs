namespace Microsoft.eShopOnContainers.Services.Ordering.API;

/// <summary>
/// 表示订单服务的配置信息类。
/// </summary>
public class OrderingSettings
{
    /// <summary>
    /// 获取或设置一个布尔值，用于指示是否使用自定义数据。
    /// 例如，可以基于此设置加载不同的数据集进行定制。
    /// </summary>
    public bool UseCustomizationData { get; set; }

    /// <summary>
    /// 获取或设置订单服务数据库的连接字符串。
    /// 该字符串用于数据库连接操作。
    /// </summary>
    public string ConnectionString { get; set; }

    /// <summary>
    /// 获取或设置事件总线的连接字符串。
    /// 通过该属性可以配置事件总线通信的连接信息。
    /// </summary>
    public string EventBusConnection { get; set; }

    /// <summary>
    /// 获取或设置服务启动后的宽限时间（以秒为单位）。
    /// 此属性通常用于在系统启动后延迟某些操作，以确保依赖项可用。
    /// </summary>
    public int GracePeriodTime { get; set; }

    /// <summary>
    /// 获取或设置检查更新的时间间隔（以秒为单位）。
    /// 该属性用于配置系统检查更新的频率。
    /// </summary>
    public int CheckUpdateTime { get; set; }
}
