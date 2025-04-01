// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityServerHost.Quickstart.UI;

public class ConsentViewModel : ConsentInputModel
{
    // 客户端名称，用于在同意页面上显示调用身份验证请求的客户端应用名称
    public string ClientName { get; set; }

    // 客户端网址，用户可通过该链接访问客户端应用
    public string ClientUrl { get; set; }

    // 客户端 Logo 的 URL，用于在同意页面上展示客户端的标志
    public string ClientLogoUrl { get; set; }

    // 指示是否允许记住用户的同意选择，方便后续请求自动应用
    public bool AllowRememberConsent { get; set; }

    // 身份相关的权限范围集合，每个 ScopeViewModel 对象代表一个权限
    public IEnumerable<ScopeViewModel> IdentityScopes { get; set; }

    // API 相关的权限范围集合，每个 ScopeViewModel 对象代表一个权限
    public IEnumerable<ScopeViewModel> ApiScopes { get; set; }
}
