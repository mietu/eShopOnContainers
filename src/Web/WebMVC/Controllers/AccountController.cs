namespace Microsoft.eShopOnContainers.WebMVC.Controllers;

/// <summary>
/// 处理用户身份验证相关操作的控制器，包括登录和注销功能
/// </summary>
[Authorize]
public class AccountController : Controller
{
    private readonly ILogger<AccountController> _logger;

    /// <summary>
    /// 初始化 <see cref="AccountController"/> 类的新实例
    /// </summary>
    /// <param name="logger">用于记录控制器操作的日志记录器</param>
    /// <exception cref="ArgumentNullException">当 <paramref name="logger"/> 为 null 时抛出</exception>
    public AccountController(ILogger<AccountController> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 处理用户登录操作，验证用户身份并获取访问令牌
    /// </summary>
    /// <param name="returnUrl">登录成功后要重定向的URL（可选）</param>
    /// <returns>重定向到目录首页的操作结果</returns>
    /// <remarks>
    /// 此方法要求用户通过 OpenID Connect 进行身份验证
    /// 成功后会记录用户信息并将访问令牌存储在 ViewData 中
    /// </remarks>
    [Authorize(AuthenticationSchemes = OpenIdConnectDefaults.AuthenticationScheme)]
    public async Task<IActionResult> SignIn(string returnUrl)
    {
        var user = User as ClaimsPrincipal;
        var token = await HttpContext.GetTokenAsync("access_token");

        _logger.LogInformation("----- User {@User} authenticated into {AppName}", user, Program.AppName);

        if (token != null)
        {
            ViewData["access_token"] = token;
        }

        // "Catalog" because UrlHelper doesn't support nameof() for controllers
        // https://github.com/aspnet/Mvc/issues/5853
        return RedirectToAction(nameof(CatalogController.Index), "Catalog");
    }

    /// <summary>
    /// 处理用户注销操作，清除用户的身份验证状态
    /// </summary>
    /// <returns>执行注销操作后的重定向结果</returns>
    /// <remarks>
    /// 此方法会同时从 Cookie 认证和 OpenID Connect 认证中注销用户
    /// 注销后会重定向回目录首页
    /// </remarks>
    public async Task<IActionResult> Signout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);

        // "Catalog" because UrlHelper doesn't support nameof() for controllers
        // https://github.com/aspnet/Mvc/issues/5853
        var homeUrl = Url.Action(nameof(CatalogController.Index), "Catalog");
        return new SignOutResult(OpenIdConnectDefaults.AuthenticationScheme,
            new AspNetCore.Authentication.AuthenticationProperties { RedirectUri = homeUrl });
    }
}
