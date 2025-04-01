namespace Microsoft.eShopOnContainers.Web.Shopping.HttpAggregator.Filters
{
    namespace Basket.API.Infrastructure.Filters
    {
        /// <summary>
        /// 用于检查API操作是否需要授权的过滤器。
        /// 如果操作上存在 Authorize 特性，则添加相应的401和403响应以及安全定义。
        /// </summary>
        public class AuthorizeCheckOperationFilter : IOperationFilter
        {
            public void Apply(OpenApiOperation operation, OperationFilterContext context)
            {
                // 使用反射检查控制器类和方法上是否存在 AuthorizeAttribute
                var hasAuthorize = context.MethodInfo.DeclaringType.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any() ||
                                   context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any();

                // 如果没有找到 Authorize 特性，直接返回，不做任何操作
                if (!hasAuthorize)
                    return;

                // 如果存在授权，添加401（未授权）响应描述
                operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Unauthorized" });
                // 添加403（禁止访问）响应描述
                operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Forbidden" });

                // 定义OAuth2安全方案引用
                var oAuthScheme = new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
                };

                // 将安全需求添加到操作中，指定所需的权限名称集合（作用域）
                operation.Security = new List<OpenApiSecurityRequirement>
                        {
                            new OpenApiSecurityRequirement
                            {
                                [ oAuthScheme ] = new[] { "Microsoft.eShopOnContainers.Web.Shopping.HttpAggregator" }
                            }
                        };
            }
        }
    }

}