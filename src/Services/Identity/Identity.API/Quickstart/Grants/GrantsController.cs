// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityServerHost.Quickstart.UI;

// 此控制器用于展示用户已授权给客户端的同意，并允许用户撤销这些授权
[SecurityHeaders]
[Authorize]
public class GrantsController : Controller
{
    // 注入的服务用于与 IdentityServer 交互、查找客户端、查找资源以及记录事件
    private readonly IIdentityServerInteractionService _interaction;
    private readonly IClientStore _clients;
    private readonly IResourceStore _resources;
    private readonly IEventService _events;

    // 构造函数，依赖注入所需的服务实例
    public GrantsController(IIdentityServerInteractionService interaction,
        IClientStore clients,
        IResourceStore resources,
        IEventService events)
    {
        _interaction = interaction;
        _clients = clients;
        _resources = resources;
        _events = events;
    }

    /// <summary>
    /// 显示所有用户授权列表
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // 调用 BuildViewModelAsync 构建视图模型并传递给视图显示
        return View("Index", await BuildViewModelAsync());
    }

    /// <summary>
    /// 处理撤销客户端授权的请求
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Revoke(string clientId)
    {
        // 撤销指定客户端的用户授权
        await _interaction.RevokeUserConsentAsync(clientId);
        // 记录撤销事件，便于审计和日志记录
        await _events.RaiseAsync(new GrantsRevokedEvent(User.GetSubjectId(), clientId));

        // 操作完成后重定向回授权列表页面
        return RedirectToAction("Index");
    }

    /// <summary>
    /// 构建视图模型，获取所有用户授权信息以及对应的客户端和资源详情
    /// </summary>
    private async Task<GrantsViewModel> BuildViewModelAsync()
    {
        // 获取当前用户所有的授权记录
        var grants = await _interaction.GetAllUserGrantsAsync();

        var list = new List<GrantViewModel>();
        foreach (var grant in grants)
        {
            // 根据授权记录中的客户端ID查找客户端详细信息
            var client = await _clients.FindClientByIdAsync(grant.ClientId);
            if (client != null)
            {
                // 根据授权范围查找对应的资源（身份资源与 API 资源）
                var resources = await _resources.FindResourcesByScopeAsync(grant.Scopes);

                // 构建单个授权的视图模型，包含客户端和资源的详细信息
                var item = new GrantViewModel()
                {
                    ClientId = client.ClientId,
                    ClientName = client.ClientName ?? client.ClientId,
                    ClientLogoUrl = client.LogoUri,
                    ClientUrl = client.ClientUri,
                    Description = grant.Description,
                    Created = grant.CreationTime,
                    Expires = grant.Expiration,
                    // 用于显示授权对应的身份资源名称
                    IdentityGrantNames = resources.IdentityResources.Select(x => x.DisplayName ?? x.Name).ToArray(),
                    // 用于显示授权对应的 API 范围名称
                    ApiGrantNames = resources.ApiScopes.Select(x => x.DisplayName ?? x.Name).ToArray()
                };

                list.Add(item);
            }
        }

        // 返回包含所有授权信息的视图模型
        return new GrantsViewModel
        {
            Grants = list
        };
    }
}
