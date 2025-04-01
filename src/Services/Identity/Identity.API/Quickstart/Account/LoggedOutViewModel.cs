// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityServerHost.Quickstart.UI;

public class LoggedOutViewModel
{
    // 注销后重定向的 URI
    public string PostLogoutRedirectUri { get; set; }

    // 客户端名称，用于在视图中显示
    public string ClientName { get; set; }

    // 用于注销的 iframe URL，支持单点注销的实现
    public string SignOutIframeUrl { get; set; }

    // 标识是否在注销后自动重定向
    public bool AutomaticRedirectAfterSignOut { get; set; }

    // 注销标识符，用于跟踪注销请求
    public string LogoutId { get; set; }

    // 如果存在外部身份验证方案，则此属性返回 true，表示需要触发外部注销操作
    public bool TriggerExternalSignout => ExternalAuthenticationScheme != null;

    // 外部身份验证方案名称，如果设置了该值则表示需要调用相应的外部注销流程
    public string ExternalAuthenticationScheme { get; set; }
}
