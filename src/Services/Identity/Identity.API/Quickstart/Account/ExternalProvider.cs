// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityServerHost.Quickstart.UI;

// ExternalProvider 类用于表示外部身份认证提供者的信息
public class ExternalProvider
{
    // DisplayName 属性存储显示名称，用于在UI中展示提供者的友好名称
    public string DisplayName { get; set; }

    // AuthenticationScheme 属性存储认证方案名称，用于标识和配置认证提供者
    public string AuthenticationScheme { get; set; }
}
