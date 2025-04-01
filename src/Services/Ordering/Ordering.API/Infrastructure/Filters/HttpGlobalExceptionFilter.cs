namespace Microsoft.eShopOnContainers.Services.Ordering.API.Infrastructure.Filters
{
    /// <summary>
    /// 全局异常过滤器，用于捕获和处理API调用过程中发生的异常。
    /// </summary>
    public class HttpGlobalExceptionFilter : IExceptionFilter
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
        /// 当异常发生时，执行处理逻辑。
        /// </summary>
        /// <param name="context">异常上下文</param>
        public void OnException(ExceptionContext context)
        {
            // 记录错误日志，使用异常的 HResult 作为事件ID
            logger.LogError(new EventId(context.Exception.HResult),
                context.Exception,
                context.Exception.Message);

            // 若异常为 OrderingDomainException 类型，则返回 400 错误及验证问题详情
            if (context.Exception.GetType() == typeof(OrderingDomainException))
            {
                // 创建验证问题详情对象，包含请求路径、状态码及详细描述
                var problemDetails = new ValidationProblemDetails()
                {
                    Instance = context.HttpContext.Request.Path,
                    Status = StatusCodes.Status400BadRequest,
                    Detail = "Please refer to the errors property for additional details."
                };

                // 添加具体领域验证错误信息
                problemDetails.Errors.Add("DomainValidations", new string[] { context.Exception.Message.ToString() });

                // 设置返回结果为 BadRequestObjectResult，并更新响应状态码
                context.Result = new BadRequestObjectResult(problemDetails);
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
            else
            {
                // 如果异常不是 OrderingDomainException，则创建一个通用的 JSON 错误响应
                var json = new JsonErrorResponse
                {
                    Messages = new[] { "An error occur.Try it again." }
                };

                // 如果当前是开发环境，则添加开发者具体异常信息
                if (env.IsDevelopment())
                {
                    json.DeveloperMessage = context.Exception;
                }

                // 设置返回结果为 InternalServerErrorObjectResult，并更新响应状态码
                // 注意：这里提到 .NET Core 1.1 的已知问题，目前在 .NET 7 中已经没有此问题
                context.Result = new InternalServerErrorObjectResult(json);
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
            // 标记异常已经被处理
            context.ExceptionHandled = true;
        }

        /// <summary>
        /// 用于返回 JSON 格式的错误响应
        /// </summary>
        private class JsonErrorResponse
        {
            /// <summary>
            /// 错误信息集合，用于向客户端展示问题
            /// </summary>
            public string[] Messages { get; set; }

            /// <summary>
            /// 开发者的错误详情，仅在开发环境返回
            /// </summary>
            public object DeveloperMessage { get; set; }
        }
    }

    // 自定义的 InternalServerErrorObjectResult，扩展 ObjectResult 用于返回 500 错误
    public class InternalServerErrorObjectResult : ObjectResult
    {
        /// <summary>
        /// 构造函数，设置状态码为 500
        /// </summary>
        /// <param name="error">错误对象</param>
        public InternalServerErrorObjectResult(object error)
            : base(error)
        {
            StatusCode = StatusCodes.Status500InternalServerError;
        }
    }

    /// <summary>
    /// 自定义领域异常类标记领域验证错误（示例，仅为说明用途）。
    /// </summary>
    public class OrderingDomainException : Exception
    {
        public OrderingDomainException(string message)
            : base(message)
        {
        }
    }
}
