namespace Basket.API.Infrastructure.Filters;

// 此类实现 IOperationFilter 接口，用于对 Swagger 生成的 API 文档进行操作过滤，
// 主要用于为需要授权验证的操作添加安全策略描述。
public class AuthorizeCheckOperationFilter : IOperationFilter
{
    // Apply 方法在 Swagger 生成文档时调用，对每个操作（接口方法）进行处理
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // 检查当前操作是否有标记 [Authorize] 特性
        // 首先检查所属的类是否标记 [Authorize] 特性，再检查方法本身是否标记 [Authorize] 特性
        var hasAuthorize = context.MethodInfo.DeclaringType.GetCustomAttributes(true)
                            .OfType<AuthorizeAttribute>().Any() ||
                            context.MethodInfo.GetCustomAttributes(true)
                            .OfType<AuthorizeAttribute>().Any();

        // 如果没有授权特性，则直接返回，不做任何安全配置
        if (!hasAuthorize)
            return;

        // 若存在授权特性，则在操作响应中添加状态码 401（未授权）和 403（禁止访问）的描述
        operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Unauthorized" });
        operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Forbidden" });

        // 配置一个 OAuth2 安全方案引用
        var oAuthScheme = new OpenApiSecurityScheme
        {
            // 指定安全方案的引用类型和标识符
            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
        };

        // 将安全策略添加到操作的安全要求中，并指定所需的作用域，例如 "basketapi"
        operation.Security = new List<OpenApiSecurityRequirement>
            {
                new()
                {
                    [oAuthScheme] = new [] { "basketapi" }
                }
            };
    }
}
