namespace Basket.API.Infrastructure.ActionResults;

// InternalServerErrorObjectResult 类用于表示返回 HTTP 500 内部服务器错误的响应结果。
// 该类继承自 ObjectResult，当传入错误对象时自动将响应状态码设置为 500。
public class InternalServerErrorObjectResult : ObjectResult
{
    // 构造函数接受一个错误对象作为参数，并将其传递给基类 ObjectResult，
    // 同时将 StatusCode 属性设置为 HttpStatusCodes.Status500InternalServerError，
    // 表示服务器端错误。
    public InternalServerErrorObjectResult(object error)
        : base(error) // 调用基类构造函数初始化错误对象
    {
        StatusCode = StatusCodes.Status500InternalServerError; // 设置状态码为 500
    }
}

