namespace Basket.API.Infrastructure.Filters;

/// <summary>
/// 全局异常过滤器，用于捕获和处理API调用过程中发生的异常。
/// </summary>
public partial class HttpGlobalExceptionFilter : IExceptionFilter
{
    // 当前环境信息，用于判断是否为开发环境
    private readonly IWebHostEnvironment env;
    // 日志记录器，用于记录错误信息
    private readonly ILogger<HttpGlobalExceptionFilter> logger;

    /// <summary>
    /// 构造函数，注入必要的依赖项。
    /// </summary>
    /// <param name="env">当前 Web 主机环境</param>
    /// <param name="logger">日志记录器</param>
    public HttpGlobalExceptionFilter(IWebHostEnvironment env, ILogger<HttpGlobalExceptionFilter> logger)
    {
        this.env = env;
        this.logger = logger;
    }

    /// <summary>
    /// 当异常发生时，执行处理逻辑，通过判断异常类型返回不同的响应。
    /// </summary>
    /// <param name="context">异常上下文</param>
    public void OnException(ExceptionContext context)
    {
        // 使用错误的HResult创建日志事件，并记录异常详细信息
        logger.LogError(new EventId(context.Exception.HResult),
            context.Exception,
            context.Exception.Message);

        // 如果异常是BasketDomainException类型
        if (context.Exception.GetType() == typeof(BasketDomainException))
        {
            // 构造错误响应，包含异常信息
            var json = new JsonErrorResponse
            {
                Messages = new[] { context.Exception.Message }
            };

            // 将结果设置为BadRequest并设置状态码为400
            context.Result = new BadRequestObjectResult(json);
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        }
        else
        {
            // 默认异常处理，构造通用错误响应信息
            var json = new JsonErrorResponse
            {
                Messages = new[] { "An error occurred. Try it again." }
            };

            // 如果是开发环境，附加开发者详细异常信息
            if (env.IsDevelopment())
            {
                json.DeveloperMessage = context.Exception;
            }

            // 将结果设置为InternalServerError并设置状态码为500
            context.Result = new InternalServerErrorObjectResult(json);
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        }
        // 标记异常已被处理，不再向上传递
        context.ExceptionHandled = true;
    }
}
