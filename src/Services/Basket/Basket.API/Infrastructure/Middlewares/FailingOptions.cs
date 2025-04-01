namespace Basket.API.Infrastructure.Middlewares;

// FailingOptions 类用于配置失败相关的选项
public class FailingOptions
{
    // ConfigPath: 默认配置路径为 "/Failing"
    public string ConfigPath = "/Failing";

    // EndpointPaths: 要过滤的端点路径列表，初始化为空列表
    public List<string> EndpointPaths { get; set; } = new List<string>();

    // NotFilteredPaths: 不需要过滤的路径列表，初始化为空列表
    public List<string> NotFilteredPaths { get; set; } = new List<string>();
}

