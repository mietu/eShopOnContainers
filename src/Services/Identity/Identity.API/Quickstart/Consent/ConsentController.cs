// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityServerHost.Quickstart.UI;

/// <summary>
/// 此控制器处理用户同意界面的相关请求
/// 使用 SecurityHeaders 和 Authorize 特性保证安全性和授权检查
/// </summary>
[SecurityHeaders]
[Authorize]
public class ConsentController : Controller
{
    // 与 IdentityServer 的交互服务，用于获取授权上下文和传递同意信息
    private readonly IIdentityServerInteractionService _interaction;
    // 事件服务，用于发布用户同意或拒绝同意事件
    private readonly IEventService _events;
    // 日志记录器
    private readonly ILogger<ConsentController> _logger;

    // 构造函数，通过依赖注入传入所需服务
    public ConsentController(
        IIdentityServerInteractionService interaction,
        IEventService events,
        ILogger<ConsentController> logger)
    {
        _interaction = interaction;
        _events = events;
        _logger = logger;
    }

    /// <summary>
    /// GET: 显示用户同意界面
    /// 根据 returnUrl 查找相关授权请求，并构造对应的视图模型返回给视图展示
    /// </summary>
    /// <param name="returnUrl">返回的 URI 地址</param>
    /// <returns>如果找到视图模型则返回"Index"视图，否则返回"Error"视图</returns>
    [HttpGet]
    public async Task<IActionResult> Index(string returnUrl)
    {
        // 构建视图模型
        var vm = await BuildViewModelAsync(returnUrl);
        if (vm != null)
        {
            return View("Index", vm);
        }

        return View("Error");
    }

    /// <summary>
    /// POST: 处理用户在同意界面点击同意或拒绝后的提交
    /// </summary>
    /// <param name="model">包含用户选择、选中的权限范围以及其它信息的输入模型</param>
    /// <returns>根据处理结果返回合适的页面或重定向</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ConsentInputModel model)
    {
        // 处理同意操作
        var result = await ProcessConsent(model);

        // 如果需要重定向（比如用户操作 OK 后返回授权端点）
        if (result.IsRedirect)
        {
            var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);
            if (context?.IsNativeClient() == true)
            {
                // 本机客户端特殊处理返回页面，改进用户体验
                return this.LoadingPage("Redirect", result.RedirectUri);
            }

            return Redirect(result.RedirectUri);
        }

        // 如果存在验证错误，则添加到 ModelState 中用于显示错误信息
        if (result.HasValidationError)
        {
            ModelState.AddModelError(string.Empty, result.ValidationError);
        }

        // 如果需要显示同意界面，则返回重新构建的视图模型
        if (result.ShowView)
        {
            return View("Index", result.ViewModel);
        }

        return View("Error");
    }

    /*****************************************/
    /* 同意控制器的辅助方法                  */
    /*****************************************/
    /// <summary>
    /// 处理用户在同意界面提交的数据，验证并生成同意响应
    /// </summary>
    /// <param name="model">用户输入模型</param>
    /// <returns>返回 ProcessConsentResult，其中包含重定向 URI、错误信息或者视图模型</returns>
    private async Task<ProcessConsentResult> ProcessConsent(ConsentInputModel model)
    {
        var result = new ProcessConsentResult();

        // 根据提交的 ReturnUrl 获取当前授权请求
        var request = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);
        if (request == null) return result;

        ConsentResponse grantedConsent = null;

        // 用户点击“不允许”
        if (model?.Button == "no")
        {
            // 生成拒绝同意的响应
            grantedConsent = new ConsentResponse { Error = AuthorizationError.AccessDenied };

            // 发送拒绝同意事件，用于日志记录或审计
            await _events.RaiseAsync(new ConsentDeniedEvent(User.GetSubjectId(), request.Client.ClientId, request.ValidatedResources.RawScopeValues));
        }
        // 用户点击“允许”
        else if (model?.Button == "yes")
        {
            // 如果用户同意了至少一个权限范围，则构建同意响应
            if (model.ScopesConsented != null && model.ScopesConsented.Any())
            {
                var scopes = model.ScopesConsented;
                // 如果离线访问未启用，则过滤掉离线访问的权限
                if (ConsentOptions.EnableOfflineAccess == false)
                {
                    scopes = scopes.Where(x => x != IdentityServerConstants.StandardScopes.OfflineAccess);
                }

                grantedConsent = new ConsentResponse
                {
                    RememberConsent = model.RememberConsent,
                    ScopesValuesConsented = scopes.ToArray(),
                    Description = model.Description
                };

                // 发送成功同意事件
                await _events.RaiseAsync(new ConsentGrantedEvent(User.GetSubjectId(), request.Client.ClientId, request.ValidatedResources.RawScopeValues, grantedConsent.ScopesValuesConsented, grantedConsent.RememberConsent));
            }
            else
            {
                // 如果用户没有选择任何权限，则设置验证错误信息
                result.ValidationError = ConsentOptions.MustChooseOneErrorMessage;
            }
        }
        else
        {
            // 如果按钮值既不是 "yes" 也不是 "no"，则设置无效选择的错误信息
            result.ValidationError = ConsentOptions.InvalidSelectionErrorMessage;
        }

        // 如果生成了有效的同意响应
        if (grantedConsent != null)
        {
            // 将同意信息更新到 IdentityServer
            await _interaction.GrantConsentAsync(request, grantedConsent);

            // 设置重定向 URI 和客户端信息
            result.RedirectUri = model.ReturnUrl;
            result.Client = request.Client;
        }
        else
        {
            // 否则需要重新显示同意界面，重新构造视图模型
            result.ViewModel = await BuildViewModelAsync(model.ReturnUrl, model);
        }

        return result;
    }

    /// <summary>
    /// 根据提交的 ReturnUrl（和可能的 ConsentInputModel）构建同意视图模型
    /// </summary>
    /// <param name="returnUrl">返回的 URI 地址</param>
    /// <param name="model">用户输入模型，可以为空</param>
    /// <returns>构建后的 consent 视图模型</returns>
    private async Task<ConsentViewModel> BuildViewModelAsync(string returnUrl, ConsentInputModel model = null)
    {
        var request = await _interaction.GetAuthorizationContextAsync(returnUrl);
        if (request != null)
        {
            return CreateConsentViewModel(model, returnUrl, request);
        }
        else
        {
            // 未找到匹配的授权请求，记录错误日志
            _logger.LogError("No consent request matching request: {0}", returnUrl);
        }

        return null;
    }

    /// <summary>
    /// 创建同意视图模型，根据授权请求和可能的用户输入数据填充视图模型数据
    /// </summary>
    /// <param name="model">用户输入的同意数据，可以为 null</param>
    /// <param name="returnUrl">返回的 URI 地址</param>
    /// <param name="request">授权请求对象</param>
    /// <returns>构造后的 ConsentViewModel 对象</returns>
    private ConsentViewModel CreateConsentViewModel(
        ConsentInputModel model, string returnUrl,
        AuthorizationRequest request)
    {
        var vm = new ConsentViewModel
        {
            // 如果用户未提交数据，则采用默认值
            RememberConsent = model?.RememberConsent ?? true,
            ScopesConsented = model?.ScopesConsented ?? Enumerable.Empty<string>(),
            Description = model?.Description,
            ReturnUrl = returnUrl,
            ClientName = request.Client.ClientName ?? request.Client.ClientId,
            ClientUrl = request.Client.ClientUri,
            ClientLogoUrl = request.Client.LogoUri,
            AllowRememberConsent = request.Client.AllowRememberConsent
        };

        // 构造身份权限范围的列表
        vm.IdentityScopes = request.ValidatedResources.Resources.IdentityResources.Select(x =>
            CreateScopeViewModel(x, vm.ScopesConsented.Contains(x.Name) || model == null)).ToArray();

        // 构造 API 权限范围的列表
        var apiScopes = new List<ScopeViewModel>();
        foreach (var parsedScope in request.ValidatedResources.ParsedScopes)
        {
            var apiScope = request.ValidatedResources.Resources.FindApiScope(parsedScope.ParsedName);
            if (apiScope != null)
            {
                var scopeVm = CreateScopeViewModel(parsedScope, apiScope, vm.ScopesConsented.Contains(parsedScope.RawValue) || model == null);
                apiScopes.Add(scopeVm);
            }
        }
        // 如果启用了离线访问，并且授权请求包含离线访问，则添加离线访问权限项
        if (ConsentOptions.EnableOfflineAccess && request.ValidatedResources.Resources.OfflineAccess)
        {
            apiScopes.Add(GetOfflineAccessScope(vm.ScopesConsented.Contains(IdentityServerConstants.StandardScopes.OfflineAccess) || model == null));
        }
        vm.ApiScopes = apiScopes;

        return vm;
    }

    /// <summary>
    /// 创建身份权限范围的视图模型
    /// </summary>
    /// <param name="identity">身份资源对象</param>
    /// <param name="check">是否默认选中该权限</param>
    /// <returns>构造后的 ScopeViewModel 对象</returns>
    private ScopeViewModel CreateScopeViewModel(IdentityResource identity, bool check)
    {
        return new ScopeViewModel
        {
            Value = identity.Name,
            DisplayName = identity.DisplayName ?? identity.Name,
            Description = identity.Description,
            Emphasize = identity.Emphasize,
            Required = identity.Required,
            // 如果权限是必需的则强制选中或者根据 check 值决定
            Checked = check || identity.Required
        };
    }

    /// <summary>
    /// 创建 API 权限范围的视图模型
    /// </summary>
    /// <param name="parsedScopeValue">解析后的权限范围值（包含参数等信息）</param>
    /// <param name="apiScope">对应的 API 权限描述</param>
    /// <param name="check">是否默认选中该权限</param>
    /// <returns>构造后的 ScopeViewModel 对象</returns>
    public ScopeViewModel CreateScopeViewModel(ParsedScopeValue parsedScopeValue, ApiScope apiScope, bool check)
    {
        // 根据参数拼接显示名称
        var displayName = apiScope.DisplayName ?? apiScope.Name;
        if (!String.IsNullOrWhiteSpace(parsedScopeValue.ParsedParameter))
        {
            displayName += ":" + parsedScopeValue.ParsedParameter;
        }

        return new ScopeViewModel
        {
            Value = parsedScopeValue.RawValue,
            DisplayName = displayName,
            Description = apiScope.Description,
            Emphasize = apiScope.Emphasize,
            Required = apiScope.Required,
            Checked = check || apiScope.Required
        };
    }

    /// <summary>
    /// 获取离线访问权限的视图模型
    /// </summary>
    /// <param name="check">是否默认选中离线访问</param>
    /// <returns>构造好的离线访问 ScopeViewModel 对象</returns>
    private ScopeViewModel GetOfflineAccessScope(bool check)
    {
        return new ScopeViewModel
        {
            Value = IdentityServerConstants.StandardScopes.OfflineAccess,
            DisplayName = ConsentOptions.OfflineAccessDisplayName,
            Description = ConsentOptions.OfflineAccessDescription,
            Emphasize = true,
            Checked = check
        };
    }
}
