// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityServerHost.Quickstart.UI;

/// <summary>
/// 设备控制器，用于处理设备授权的请求、用户设备验证码确认以及用户同意页面的交互
/// </summary>
[Authorize]
[SecurityHeaders]
public class DeviceController : Controller
{
    // 处理设备流交互的服务
    private readonly IDeviceFlowInteractionService _interaction;
    // 事件服务，用于记录和发送同意相关的事件
    private readonly IEventService _events;
    // IdentityServer的配置信息
    private readonly IOptions<IdentityServerOptions> _options;
    // 日志记录器
    private readonly ILogger<DeviceController> _logger;

    /// <summary>
    /// 构造函数，注入必要的服务
    /// </summary>
    /// <param name="interaction">设备流交互服务</param>
    /// <param name="eventService">事件服务</param>
    /// <param name="options">IdentityServer选项</param>
    /// <param name="logger">日志记录器</param>
    public DeviceController(
        IDeviceFlowInteractionService interaction,
        IEventService eventService,
        IOptions<IdentityServerOptions> options,
        ILogger<DeviceController> logger)
    {
        _interaction = interaction;
        _events = eventService;
        _options = options;
        _logger = logger;
    }

    /// <summary>
    /// 处理GET请求，展示用户验证码输入或确认页面
    /// </summary>
    /// <returns>返回对应视图</returns>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // 从配置中读取用户验证码查询参数名称
        string userCodeParamName = _options.Value.UserInteraction.DeviceVerificationUserCodeParameter;
        // 从请求查询字符串获取用户输入的验证码
        string userCode = Request.Query[userCodeParamName];
        // 若验证码为空，则显示验证码输入页面
        if (string.IsNullOrWhiteSpace(userCode))
            return View("UserCodeCapture");

        // 构建视图模型
        var vm = await BuildViewModelAsync(userCode);
        if (vm == null)
            return View("Error");

        // 标记需要确认用户验证码
        vm.ConfirmUserCode = true;
        return View("UserCodeConfirmation", vm);
    }

    /// <summary>
    /// 处理POST请求，用户提交验证码
    /// </summary>
    /// <param name="userCode">用户输入的验证码</param>
    /// <returns>展示验证码确认页面</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UserCodeCapture(string userCode)
    {
        // 根据验证码构建视图模型
        var vm = await BuildViewModelAsync(userCode);
        if (vm == null)
            return View("Error");

        // 跳转到验证码确认页面
        return View("UserCodeConfirmation", vm);
    }

    /// <summary>
    /// 处理用户同意的回调请求，提交同意或拒绝的结果
    /// </summary>
    /// <param name="model">用户同意信息模型</param>
    /// <returns>根据处理结果返回错误页面或成功页面</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Callback(DeviceAuthorizationInputModel model)
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        // 处理用户的同意请求
        var result = await ProcessConsent(model);
        if (result.HasValidationError)
            return View("Error");

        return View("Success");
    }

    /// <summary>
    /// 根据用户提交的同意模型处理同意逻辑
    /// </summary>
    /// <param name="model">包含用户选择和范围信息的输入模型</param>
    /// <returns>返回包含处理结果的对象</returns>
    private async Task<ProcessConsentResult> ProcessConsent(DeviceAuthorizationInputModel model)
    {
        var result = new ProcessConsentResult();

        // 根据验证码获取授权上下文
        var request = await _interaction.GetAuthorizationContextAsync(model.UserCode);
        if (request == null)
            return result;

        ConsentResponse grantedConsent = null;

        // 用户点击“拒绝”，返回标准“access_denied”响应
        if (model.Button == "no")
        {
            grantedConsent = new ConsentResponse { Error = AuthorizationError.AccessDenied };

            // 记录拒绝同意事件
            await _events.RaiseAsync(new ConsentDeniedEvent(
                User.GetSubjectId(),
                request.Client.ClientId,
                request.ValidatedResources.RawScopeValues));
        }
        // 用户点击“同意”
        else if (model.Button == "yes")
        {
            // 如果用户选择了一些权限范围，则构建响应模型
            if (model.ScopesConsented != null && model.ScopesConsented.Any())
            {
                var scopes = model.ScopesConsented;
                // 若不允许离线访问，则过滤离线访问权限
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

                // 记录同意事件
                await _events.RaiseAsync(new ConsentGrantedEvent(
                    User.GetSubjectId(),
                    request.Client.ClientId,
                    request.ValidatedResources.RawScopeValues,
                    grantedConsent.ScopesValuesConsented,
                    grantedConsent.RememberConsent));
            }
            else
            {
                // 没有选择任何权限，返回错误提示
                result.ValidationError = ConsentOptions.MustChooseOneErrorMessage;
            }
        }
        else
        {
            // 非法选择，返回错误提示
            result.ValidationError = ConsentOptions.InvalidSelectionErrorMessage;
        }

        if (grantedConsent != null)
        {
            // 将同意结果传递给IdentityServer
            await _interaction.HandleRequestAsync(model.UserCode, grantedConsent);

            // 配置重定向地址和客户端信息
            result.RedirectUri = model.ReturnUrl;
            result.Client = request.Client;
        }
        else
        {
            // 需要重新显示同意页面
            result.ViewModel = await BuildViewModelAsync(model.UserCode, model);
        }

        return result;
    }

    /// <summary>
    /// 根据用户验证码构建设备授权视图模型
    /// </summary>
    /// <param name="userCode">用户输入的验证码</param>
    /// <param name="model">可选的用户同意输入模型</param>
    /// <returns>返回设备授权视图模型</returns>
    private async Task<DeviceAuthorizationViewModel> BuildViewModelAsync(string userCode, DeviceAuthorizationInputModel model = null)
    {
        var request = await _interaction.GetAuthorizationContextAsync(userCode);
        if (request != null)
        {
            // 调用内部方法构建视图模型
            return CreateConsentViewModel(userCode, model, request);
        }

        return null;
    }

    /// <summary>
    /// 使用授权请求数据和用户输入数据创建设备授权视图模型
    /// </summary>
    /// <param name="userCode">用户验证码</param>
    /// <param name="model">用户的同意输入模型</param>
    /// <param name="request">设备流授权请求上下文</param>
    /// <returns>返回构建好的视图模型</returns>
    private DeviceAuthorizationViewModel CreateConsentViewModel(string userCode, DeviceAuthorizationInputModel model, DeviceFlowAuthorizationRequest request)
    {
        var vm = new DeviceAuthorizationViewModel
        {
            UserCode = userCode,
            Description = model?.Description,
            RememberConsent = model?.RememberConsent ?? true,
            ScopesConsented = model?.ScopesConsented ?? Enumerable.Empty<string>(),
            ClientName = request.Client.ClientName ?? request.Client.ClientId,
            ClientUrl = request.Client.ClientUri,
            ClientLogoUrl = request.Client.LogoUri,
            AllowRememberConsent = request.Client.AllowRememberConsent
        };

        // 构建设备授权中涉及的身份范围列表
        vm.IdentityScopes = request.ValidatedResources.Resources.IdentityResources
            .Select(x => CreateScopeViewModel(x, vm.ScopesConsented.Contains(x.Name) || model == null))
            .ToArray();

        var apiScopes = new List<ScopeViewModel>();

        // 遍历解析后的权限范围，根据相应的API权限创建视图模型
        foreach (var parsedScope in request.ValidatedResources.ParsedScopes)
        {
            var apiScope = request.ValidatedResources.Resources.FindApiScope(parsedScope.ParsedName);
            if (apiScope != null)
            {
                var scopeVm = CreateScopeViewModel(parsedScope, apiScope, vm.ScopesConsented.Contains(parsedScope.RawValue) || model == null);
                apiScopes.Add(scopeVm);
            }
        }
        // 如果允许离线访问且支持离线访问，则添加离线访问权限
        if (ConsentOptions.EnableOfflineAccess && request.ValidatedResources.Resources.OfflineAccess)
        {
            apiScopes.Add(GetOfflineAccessScope(vm.ScopesConsented.Contains(IdentityServerConstants.StandardScopes.OfflineAccess) || model == null));
        }
        vm.ApiScopes = apiScopes;

        return vm;
    }

    /// <summary>
    /// 根据IdentityResource创建ScopeViewModel，并确定是否默认选中
    /// </summary>
    /// <param name="identity">身份资源</param>
    /// <param name="check">是否选中</param>
    /// <returns>返回ScopeViewModel对象</returns>
    private ScopeViewModel CreateScopeViewModel(IdentityResource identity, bool check)
    {
        return new ScopeViewModel
        {
            Value = identity.Name,
            DisplayName = identity.DisplayName ?? identity.Name,
            Description = identity.Description,
            Emphasize = identity.Emphasize,
            Required = identity.Required,
            Checked = check || identity.Required
        };
    }

    /// <summary>
    /// 根据解析后的权限值和ApiScope创建ScopeViewModel，并确定是否默认选中
    /// </summary>
    /// <param name="parsedScopeValue">解析后的权限值</param>
    /// <param name="apiScope">对应的API权限</param>
    /// <param name="check">是否选中</param>
    /// <returns>返回ScopeViewModel对象</returns>
    public ScopeViewModel CreateScopeViewModel(ParsedScopeValue parsedScopeValue, ApiScope apiScope, bool check)
    {
        return new ScopeViewModel
        {
            Value = parsedScopeValue.RawValue,
            DisplayName = apiScope.DisplayName ?? apiScope.Name,
            Description = apiScope.Description,
            Emphasize = apiScope.Emphasize,
            Required = apiScope.Required,
            Checked = check || apiScope.Required
        };
    }

    /// <summary>
    /// 创建离线访问权限的ScopeViewModel
    /// </summary>
    /// <param name="check">是否默认选中</param>
    /// <returns>返回离线访问ScopeViewModel对象</returns>
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
