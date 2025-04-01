namespace WebhookClient.Controllers;

/// <summary>
/// 处理用户账户认证相关操作的控制器
/// </summary>
[Authorize] // 要求用户必须经过身份验证才能访问此控制器中的操作
public class AccountController : Controller
{
    /// <summary>
    /// 处理用户登录操作
    /// </summary>
    /// <param name="returnUrl">登录成功后要重定向的URL</param>
    /// <returns>重定向到首页的操作结果</returns>
    public async Task<IActionResult> SignIn(string returnUrl)
    {
        // 获取当前已认证用户的身份信息
        var user = User as ClaimsPrincipal;

        // 从当前HTTP上下文中获取访问令牌
        var token = await HttpContext.GetTokenAsync("access_token");

        // 如果令牌存在，则将其存储在ViewData中，使其可在视图中访问
        if (token != null)
        {
            ViewData["access_token"] = token;
        }

        // 登录成功后重定向到首页
        return RedirectToPage("/Index");
    }

    /// <summary>
    /// 处理用户登出操作
    /// </summary>
    /// <returns>登出操作的结果</returns>
    public async Task<IActionResult> Signout()
    {
        // 从Cookie认证方案中退出登录
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        // 从OpenIdConnect认证方案中退出登录
        await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);

        // 获取首页的URL
        var homeUrl = Url.Page("/Index");

        // 返回登出结果，并指定登出后重定向到首页
        return new SignOutResult(OpenIdConnectDefaults.AuthenticationScheme,
            new AuthenticationProperties { RedirectUri = homeUrl });
    }
}
