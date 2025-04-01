namespace Microsoft.eShopOnContainers.Payment.API;

/// <summary>
/// 支付服务配置类
/// </summary>
public class PaymentSettings
{
    /// <summary>
    /// 获取或设置支付是否成功
    /// 用于模拟支付结果，在开发和测试环境中特别有用
    /// </summary>
    public bool PaymentSucceeded { get; set; }

    /// <summary>
    /// 获取或设置事件总线连接字符串
    /// 用于与其他微服务进行通信，发布支付完成事件
    /// </summary>
    public string EventBusConnection { get; set; }
}

