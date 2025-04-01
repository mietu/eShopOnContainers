namespace Basket.API.Infrastructure.Filters;

/// <summary>
/// JsonErrorResponse 类用于封装 API 错误响应。
/// </summary>
public class JsonErrorResponse
{
    /// <summary>
    /// 用户可见的错误消息数组。
    /// 用于向用户展示错误原因。
    /// </summary>
    public string[] Messages { get; set; }

    /// <summary>
    /// 开发者的错误信息。
    /// 可以包含详细的错误堆栈或调试信息，仅供开发调试使用。
    /// </summary>
    public object DeveloperMessage { get; set; }
}

