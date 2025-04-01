namespace Microsoft.eShopOnContainers.Services.Basket.API.Auth.Server;

/// <summary>
/// 此操作过滤器用于在 Swagger/OpenAPI 文档中为需要授权的 API 添加 Authorization 头参数。
/// </summary>
public class AuthorizationHeaderParameterOperationFilter : IOperationFilter
{
    /// <summary>
    /// 应用操作过滤器，在 API 描述中检测是否需要授权，然后添加 Authorization 头参数。
    /// </summary>
    /// <param name="operation">当前 API 操作的 OpenAPI 操作对象。</param>
    /// <param name="context">包含 API 描述及相关过滤器上下文的信息。</param>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // 获取当前 API 操作的所有过滤器描述
        var filterPipeline = context.ApiDescription.ActionDescriptor.FilterDescriptors;

        // 检查是否存在授权过滤器（即需要授权的筛选条件）
        var isAuthorized = filterPipeline
            .Select(filterInfo => filterInfo.Filter)
            .Any(filter => filter is AuthorizeFilter);

        // 检查是否存在允许匿名访问的过滤器
        var allowAnonymous = filterPipeline
            .Select(filterInfo => filterInfo.Filter)
            .Any(filter => filter is IAllowAnonymousFilter);

        // 如果 API 需要授权且不允许匿名访问，则为该 API 操作添加 Authorization 头参数
        if (isAuthorized && !allowAnonymous)
        {
            // 确保操作参数集合已初始化
            operation.Parameters ??= new List<OpenApiParameter>();

            // 添加 Authorization 参数，要求客户端在请求头中传入访问令牌
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "Authorization",
                In = ParameterLocation.Header,
                Description = "access token",
                Required = true
            });
        }
    }
}
