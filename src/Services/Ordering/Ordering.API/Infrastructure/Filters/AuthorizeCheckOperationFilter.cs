namespace Microsoft.eShopOnContainers.Services.Ordering.API.Infrastructure.Filters;

/// <summary>
/// 该操作过滤器用于在生成 OpenAPI 文档时，根据控制器或方法上的 AuthorizeAttribute 增加安全性配置。
/// </summary>
public class AuthorizeCheckOperationFilter : IOperationFilter
{
    /// <summary>
    /// 根据给定的上下文，检查是否需要添加 401/403 响应及安全性要求。
    /// </summary>
    /// <param name="operation">当前 OpenAPI 操作对象</param>
    /// <param name="context">操作过滤器的上下文，包含方法信息等</param>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // 检查当前方法所在的类或方法上是否存在 AuthorizeAttribute
        var hasAuthorize = context.MethodInfo.DeclaringType.GetCustomAttributes(true)
                              .OfType<AuthorizeAttribute>().Any() ||
                           context.MethodInfo.GetCustomAttributes(true)
                              .OfType<AuthorizeAttribute>().Any();

        // 如果没有认证要求，则直接返回，不做任何修改
        if (!hasAuthorize)
            return;

        // 如果存在认证要求，则为操作添加 401 Unauthorized 响应
        operation.Responses.TryAdd("401", new OpenApiResponse { Description = "未授权" });
        // 为操作添加 403 Forbidden 响应
        operation.Responses.TryAdd("403", new OpenApiResponse { Description = "禁止访问" });

        // 配置 OAuth2 认证方案，该方案的引用 ID 为 "oauth2"
        var oAuthScheme = new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
        };

        // 指定该操作所使用的安全要求，这里定义了使用 "orderingapi" 范围
        operation.Security = new List<OpenApiSecurityRequirement>
            {
                new()
                {
                    [ oAuthScheme ] = new [] { "orderingapi" }
                }
            };
    }
}
