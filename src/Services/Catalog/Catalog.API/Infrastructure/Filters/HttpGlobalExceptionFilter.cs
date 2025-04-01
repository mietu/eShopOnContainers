namespace Microsoft.eShopOnContainers.Services.Catalog.API.Infrastructure.Filters;

// 定义全局异常过滤器，捕获并处理API调用过程中产生的异常
public class HttpGlobalExceptionFilter : IExceptionFilter
{
    // 当前Web主机环境，用于判断是否处于开发环境
    private readonly IWebHostEnvironment env;
    // 日志记录器，用于记录错误详细信息
    private readonly ILogger<HttpGlobalExceptionFilter> logger;

    // 构造函数，注入IWebHostEnvironment和ILogger依赖项
    public HttpGlobalExceptionFilter(IWebHostEnvironment env, ILogger<HttpGlobalExceptionFilter> logger)
    {
        this.env = env;
        this.logger = logger;
    }

    // 当发生异常时，执行此方法
    public void OnException(ExceptionContext context)
    {
        // 记录错误日志，包含错误标识、异常信息和异常消息
        logger.LogError(new EventId(context.Exception.HResult),
            context.Exception,
            context.Exception.Message);

        // 根据异常类型进行处理
        if (context.Exception.GetType() == typeof(CatalogDomainException))
        {
            // 如果是CatalogDomainException异常，构造ValidationProblemDetails对象
            var problemDetails = new ValidationProblemDetails()
            {
                Instance = context.HttpContext.Request.Path,                       // 请求路径
                Status = StatusCodes.Status400BadRequest,                           // 状态码400
                Detail = "Please refer to the errors property for additional details." // 错误详情提示
            };

            // 将具体错误信息添加到Errors集合中
            problemDetails.Errors.Add("DomainValidations", new string[] { context.Exception.Message.ToString() });

            // 设置返回结果为BadRequest，通过ValidationProblemDetails返回错误信息
            context.Result = new BadRequestObjectResult(problemDetails);
            // 设置响应状态码为400
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        }
        else
        {
            // 对于其他类型异常，构造JsonErrorResponse对象
            var json = new JsonErrorResponse
            {
                Messages = new[] { "An error ocurred." } // 返回通用错误提示
            };

            // 如果当前处于开发环境，返回详细的异常信息
            if (env.IsDevelopment())
            {
                json.DeveloperMessage = context.Exception;
            }

            // 设置返回结果为InternalServerErrorObjectResult，封装错误信息
            context.Result = new InternalServerErrorObjectResult(json);
            // 设置响应状态码为500
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        }
        // 标记异常已被处理，防止后续中间件重复处理此异常
        context.ExceptionHandled = true;
    }

    // 内部类，用于封装JSON格式的错误响应
    private class JsonErrorResponse
    {
        // 错误消息集合，向客户端返回的错误提示信息
        public string[] Messages { get; set; }
        // 开发者错误信息，仅在开发环境返回用于调试
        public object DeveloperMessage { get; set; }
    }
}
