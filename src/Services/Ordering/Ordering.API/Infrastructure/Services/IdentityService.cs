namespace Microsoft.eShopOnContainers.Services.Ordering.API.Infrastructure.Services;

public class IdentityService : IIdentityService
{
    // IHttpContextAccessor 用于访问当前 HTTP 上下文
    private IHttpContextAccessor _context;

    // 构造函数，注入 IHttpContextAccessor 实例，如果传入 null 则抛出异常
    public IdentityService(IHttpContextAccessor context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    // 获取当前用户的唯一身份标识，此处从 JWT 中的 "sub" 声明获取
    public string GetUserIdentity()
    {
        return _context.HttpContext.User.FindFirst("sub").Value;
    }

    // 获取当前用户的用户名，此处直接从用户身份的 Name 属性获取
    public string GetUserName()
    {
        return _context.HttpContext.User.Identity.Name;
    }
}
