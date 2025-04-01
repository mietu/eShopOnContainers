namespace Microsoft.eShopOnContainers.Services.Ordering.API.Infrastructure.Auth;

// 实现了 IOperationFilter 接口，用于在生成 OpenAPI 文档时对操作进行过滤与修改
public class AuthorizationHeaderParameterOperationFilter : IOperationFilter
{
    // Apply 方法在每个操作被处理时调用
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // 从 Api 描述中获取当前 Action 对应的所有过滤器描述信息
        var filterPipeline = context.ApiDescription.ActionDescriptor.FilterDescriptors;

        // 判断当前操作是否需要授权：查找是否存在 AuthorizeFilter
        var isAuthorized = filterPipeline
            .Select(filterInfo => filterInfo.Filter)
            .Any(filter => filter is AuthorizeFilter);

        // 判断当前操作是否允许匿名访问：查找是否存在 IAllowAnonymousFilter
        var allowAnonymous = filterPipeline
            .Select(filterInfo => filterInfo.Filter)
            .Any(filter => filter is IAllowAnonymousFilter);

        // 如果需要授权并且不允许匿名访问，则在操作中添加授权头参数
        if (isAuthorized && !allowAnonymous)
        {
            // 如果操作参数为空，则初始化一个新的 List
            if (operation.Parameters == null)
                operation.Parameters = new List<OpenApiParameter>();

            // 添加一个新的 OpenApiParameter，表示需要传入的 Authorization 头
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "Authorization",               // 参数名
                In = ParameterLocation.Header,        // 参数位置在 HTTP 头中
                Description = "access token",         // 参数描述
                Required = true                       // 此参数为必填项
            });
        }
    }
}
