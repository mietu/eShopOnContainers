// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityServerHost.Quickstart.UI;

// LoginViewModel 类继承自 LoginInputModel，扩展了登录页面的功能
public class LoginViewModel : LoginInputModel
{
    // 允许用户记住登录状态，默认为 true
    public bool AllowRememberLogin { get; set; } = true;

    // 是否启用本地登录（用户名/密码方式），默认为 true
    public bool EnableLocalLogin { get; set; } = true;

    // 外部登录提供程序列表，用于存储第三方登录信息，初始为空集合
    public IEnumerable<ExternalProvider> ExternalProviders { get; set; } = Enumerable.Empty<ExternalProvider>();

    // 筛选出具有有效显示名称的外部登录提供程序（即需要显示在登录页面上的）
    public IEnumerable<ExternalProvider> VisibleExternalProviders => ExternalProviders
        .Where(x => !string.IsNullOrWhiteSpace(x.DisplayName));

    // 判断是否仅允许外部登录的：当本地登录功能被禁用且仅配置了一个外部登录提供程序时，返回 true
    public bool IsExternalLoginOnly => EnableLocalLogin == false
                                        && ExternalProviders?.Count() == 1;

    // 如果仅允许外部登录，则返回外部登录提供程序的认证方案，否则返回 null
    public string ExternalLoginScheme => IsExternalLoginOnly
                                         ? ExternalProviders?.SingleOrDefault()?.AuthenticationScheme
                                         : null;
}
