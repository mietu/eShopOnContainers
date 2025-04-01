namespace Microsoft.eShopOnContainers.Services.Basket.API.Services;

// IdentityService类实现了IIdentityService接口，负责处理用户身份相关操作
public class IdentityService : IIdentityService
{
    // IHttpContextAccessor用于访问当前HTTP请求的上下文信息
    private readonly IHttpContextAccessor _context;

    // 构造函数注入IHttpContextAccessor依赖，当参数为null时抛出异常
    public IdentityService(IHttpContextAccessor context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    // GetUserIdentity方法获取当前用户的唯一标识符
    public string GetUserIdentity()
    {
        // 从HttpContext中获取用户Claims，查找"sub"（subject）声明并返回其值
        return _context.HttpContext.User.FindFirst("sub").Value;
    }
}

