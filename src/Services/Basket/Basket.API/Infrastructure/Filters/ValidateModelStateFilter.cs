namespace Basket.API.Infrastructure.Filters;

// 自定义模型验证过滤器：继承自 ActionFilterAttribute
public class ValidateModelStateFilter : ActionFilterAttribute
{
    // 重写 OnActionExecuting 方法，在Action方法执行前执行
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        // 如果模型状态有效则直接返回，无需处理错误信息
        if (context.ModelState.IsValid)
        {
            return;
        }

        // 当模型状态无效时，遍历 ModelState 中的错误信息，并抽取所有的错误消息
        var validationErrors = context.ModelState
            .Keys                              // 遍历所有的键
            .SelectMany(k => context.ModelState[k].Errors) // 获取每个键对应的错误集合
            .Select(e => e.ErrorMessage)         // 提取每个错误的错误信息
            .ToArray();                        // 转换为数组

        // 构造一个 JSON 错误响应的对象，其中包含所有的错误消息
        var json = new JsonErrorResponse
        {
            Messages = validationErrors
        };

        // 设置上下文的返回结果为 BadRequestObjectResult，这将返回 HTTP 400 状态码和错误信息
        context.Result = new BadRequestObjectResult(json);
    }
}

