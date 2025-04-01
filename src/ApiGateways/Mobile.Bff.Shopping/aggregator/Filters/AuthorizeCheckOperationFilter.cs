namespace Microsoft.eShopOnContainers.Mobile.Shopping.HttpAggregator.Filters
{
    namespace Basket.API.Infrastructure.Filters
    {
        // AuthorizeCheckOperationFilter 类实现了 IOperationFilter 接口
        // 该类用于在 API 文档中添加安全相关的响应和安全要求
        public class AuthorizeCheckOperationFilter : IOperationFilter
        {
            // Apply 方法用于检查当前操作是否具有授权属性，并为 Swagger 操作文档作相应配置
            public void Apply(OpenApiOperation operation, OperationFilterContext context)
            {
                // 检查当前方法或其声明类型上是否存在 AuthorizeAttribute 授权属性
                var hasAuthorize = context.MethodInfo.DeclaringType.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any() ||
                                   context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any();

                // 如果没有授权属性，则不需要做额外处理，直接返回
                if (!hasAuthorize) return;

                // 为操作添加 401 响应（未授权）
                operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Unauthorized" });
                // 为操作添加 403 响应（禁止访问）
                operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Forbidden" });

                // 定义 OAuth2 安全方案，在引用中指定 Id 为 "oauth2"
                var oAuthScheme = new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
                };

                // 将安全要求添加到操作中，表明该操作需要 OAuth2 验证，并指定相关权限范围
                operation.Security = new List<OpenApiSecurityRequirement>
                        {
                            new()
                            {
                                [ oAuthScheme ] = new [] { "Microsoft.eShopOnContainers.Mobile.Shopping.HttpAggregator" }
                            }
                        };
            }
        }
    }
}
