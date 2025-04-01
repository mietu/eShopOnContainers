namespace Microsoft.eShopOnContainers.Services.Catalog.API.Infrastructure.ActionResults;

/// <summary>
/// 自定义的 ActionResult，用于返回内部服务器错误（500）响应。
/// </summary>
public class InternalServerErrorObjectResult : ObjectResult
{
    /// <summary>
    /// 构造函数，接收一个错误对象并设置 HTTP 状态码为 500。
    /// </summary>
    /// <param name="error">错误信息对象</param>
    public InternalServerErrorObjectResult(object error)
        : base(error)
    {
        // 设置 HTTP 状态码为 500（内部服务器错误）
        StatusCode = StatusCodes.Status500InternalServerError;
    }
}
