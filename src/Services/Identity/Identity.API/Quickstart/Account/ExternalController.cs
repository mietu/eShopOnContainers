namespace IdentityServerHost.Quickstart.UI;

// 添加安全性请求头，并允许匿名访问
[SecurityHeaders]
[AllowAnonymous]
public class ExternalController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IIdentityServerInteractionService _interaction;
    private readonly IClientStore _clientStore;
    private readonly IEventService _events;
    private readonly ILogger<ExternalController> _logger;

    // 构造函数，注入相关的服务
    public ExternalController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IIdentityServerInteractionService interaction,
        IClientStore clientStore,
        IEventService events,
        ILogger<ExternalController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _interaction = interaction;
        _clientStore = clientStore;
        _events = events;
        _logger = logger;
    }

    /// <summary>
    /// 发起对外部认证提供程序的请求，进行身份验证回合程序
    /// </summary>
    /// <param name="scheme">认证方案</param>
    /// <param name="returnUrl">返回URL</param>
    /// <returns>重定向到外部认证提供程序</returns>
    [HttpGet]
    public IActionResult Challenge(string scheme, string returnUrl)
    {
        // 如果返回URL为空则设为根路径
        if (string.IsNullOrEmpty(returnUrl)) returnUrl = "~/";

        // 验证返回URL，必须是本地URL或合法的OIDC URL
        if (Url.IsLocalUrl(returnUrl) == false && _interaction.IsValidReturnUrl(returnUrl) == false)
        {
            // 如果返回URL不合法，抛出异常
            throw new Exception("invalid return URL");
        }

        // 构造认证属性，指定回调地址，同时传递返回URL和方案信息
        var props = new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(Callback)),
            Items =
                {
                    { "returnUrl", returnUrl },
                    { "scheme", scheme },
                }
        };

        // 发起挑战，重定向到外部认证提供程序页面
        return Challenge(props, scheme);
    }

    /// <summary>
    /// 外部认证完成后的回调处理
    /// </summary>
    /// <returns>返回最终的跳转结果</returns>
    [HttpGet]
    public async Task<IActionResult> Callback()
    {
        // 从临时Cookie中读取外部认证信息
        var result = await HttpContext.AuthenticateAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);
        if (result?.Succeeded != true)
        {
            throw new Exception("External authentication error");
        }

        // 如果日志级别为调试，则记录所有外部认证的Claims信息
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            var externalClaims = result.Principal.Claims.Select(c => $"{c.Type}: {c.Value}");
            _logger.LogDebug("External claims: {@claims}", externalClaims);
        }

        // 根据外部认证信息查找或自动创建本地用户
        var (user, provider, providerUserId, claims) = await FindUserFromExternalProviderAsync(result);
        if (user == null)
        {
            // 如果未找到用户，则自动创建新的外部用户
            user = await AutoProvisionUserAsync(provider, providerUserId, claims);
        }

        // 收集其他额外的Claims以及认证属性
        var additionalLocalClaims = new List<Claim>();
        var localSignInProps = new AuthenticationProperties();
        ProcessLoginCallback(result, additionalLocalClaims, localSignInProps);

        // 生成本地用户的Claims principal
        var principal = await _signInManager.CreateUserPrincipalAsync(user);
        additionalLocalClaims.AddRange(principal.Claims);
        var name = principal.FindFirst(JwtClaimTypes.Name)?.Value ?? user.Id;

        // 构造IdentityServer的用户对象，包含额外的Claims和身份提供者信息
        var isuser = new IdentityServerUser(user.Id)
        {
            DisplayName = name,
            IdentityProvider = provider,
            AdditionalClaims = additionalLocalClaims
        };

        // 发出本地认证Cookie
        await HttpContext.SignInAsync(isuser, localSignInProps);

        // 删除临时用的外部认证Cookie
        await HttpContext.SignOutAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);

        // 获取返回URL，如果不存在则默认跳转到根路径
        var returnUrl = result.Properties.Items["returnUrl"] ?? "~/";

        // 检查是否为OIDC请求，并记录登录成功事件
        var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
        await _events.RaiseAsync(new UserLoginSuccessEvent(provider, providerUserId, user.Id, name, true, context?.Client.ClientId));

        if (context != null)
        {
            // 针对原生客户端，显示加载页面以提升用户体验
            if (context.IsNativeClient())
            {
                return this.LoadingPage("Redirect", returnUrl);
            }
        }

        // 重定向到原始请求的返回URL
        return Redirect(returnUrl);
    }

    // 从外部认证结果中解析出用户和相关信息
    private async Task<(ApplicationUser user, string provider, string providerUserId, IEnumerable<Claim> claims)>
        FindUserFromExternalProviderAsync(AuthenticateResult result)
    {
        var externalUser = result.Principal;

        // 根据常见Claim（如sub或NameIdentifier）获取外部用户唯一标识
        var userIdClaim = externalUser.FindFirst(JwtClaimTypes.Subject) ??
                          externalUser.FindFirst(ClaimTypes.NameIdentifier) ??
                          throw new Exception("Unknown userid");

        // 将唯一标识Claim移除，避免其作为额外Claim存入用户数据中
        var claims = externalUser.Claims.ToList();
        claims.Remove(userIdClaim);

        // 从认证属性中获取所使用的外部认证方案
        var provider = result.Properties.Items["scheme"];
        var providerUserId = userIdClaim.Value;

        // 查询是否存在与该外部登录相关联的本地用户
        var user = await _userManager.FindByLoginAsync(provider, providerUserId);

        return (user, provider, providerUserId, claims);
    }

    // 自动为外部用户创建本地用户账号，并关联外部登录信息
    private async Task<ApplicationUser> AutoProvisionUserAsync(string provider, string providerUserId, IEnumerable<Claim> claims)
    {
        // 筛选需要传递到用户存储中的Claims
        var filtered = new List<Claim>();

        // 获取用户显示名称
        var name = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Name)?.Value ??
                   claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
        if (name != null)
        {
            filtered.Add(new Claim(JwtClaimTypes.Name, name));
        }
        else
        {
            // 如果没有完整名称，尝试通过FirstName和LastName组合显示名称
            var first = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.GivenName)?.Value ??
                        claims.FirstOrDefault(x => x.Type == ClaimTypes.GivenName)?.Value;
            var last = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.FamilyName)?.Value ??
                       claims.FirstOrDefault(x => x.Type == ClaimTypes.Surname)?.Value;
            if (first != null && last != null)
            {
                filtered.Add(new Claim(JwtClaimTypes.Name, first + " " + last));
            }
            else if (first != null)
            {
                filtered.Add(new Claim(JwtClaimTypes.Name, first));
            }
            else if (last != null)
            {
                filtered.Add(new Claim(JwtClaimTypes.Name, last));
            }
        }

        // 获取并添加邮箱信息
        var email = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Email)?.Value ??
                    claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
        if (email != null)
        {
            filtered.Add(new Claim(JwtClaimTypes.Email, email));
        }

        // 创建新的ApplicationUser实例，使用Guid作为用户名确保唯一性
        var user = new ApplicationUser
        {
            UserName = Guid.NewGuid().ToString(),
        };
        var identityResult = await _userManager.CreateAsync(user);
        if (!identityResult.Succeeded) throw new Exception(identityResult.Errors.First().Description);

        // 将筛选后的Claims添加到用户中
        if (filtered.Any())
        {
            identityResult = await _userManager.AddClaimsAsync(user, filtered);
            if (!identityResult.Succeeded) throw new Exception(identityResult.Errors.First().Description);
        }

        // 将外部登录信息与本地用户关联
        identityResult = await _userManager.AddLoginAsync(user, new UserLoginInfo(provider, providerUserId, provider));
        if (!identityResult.Succeeded) throw new Exception(identityResult.Errors.First().Description);

        return user;
    }

    // 处理外部登录回调所需的额外数据处理逻辑，传递session id和id_token用于单点注销
    private void ProcessLoginCallback(AuthenticateResult externalResult, List<Claim> localClaims, AuthenticationProperties localSignInProps)
    {
        // 如果外部系统提供了session id，则复制到本地Claims中
        var sid = externalResult.Principal.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.SessionId);
        if (sid != null)
        {
            localClaims.Add(new Claim(JwtClaimTypes.SessionId, sid.Value));
        }

        // 如果外部提供者颁发了id_token，将其存储到认证属性中以便后续注销使用
        var idToken = externalResult.Properties.GetTokenValue("id_token");
        if (idToken != null)
        {
            localSignInProps.StoreTokens(new[] { new AuthenticationToken { Name = "id_token", Value = idToken } });
        }
    }
}
