// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityServerHost.Quickstart.UI;

public class AccountOptions
{
    // 是否允许本地登录。true 表示允许用户使用本地账号登录。
    public static bool AllowLocalLogin = true;

    // 是否允许记住登录状态。true 表示支持“记住我”功能。
    public static bool AllowRememberLogin = true;

    // “记住我”登录状态的有效持续时间，此处设定为 30 天。
    public static TimeSpan RememberMeLoginDuration = TimeSpan.FromDays(30);

    // 是否在登出时显示确认提示。false 表示登出时不显示提示页面。
    public static bool ShowLogoutPrompt = false;

    // 登出后是否自动重定向。true 表示自动重定向。
    public static bool AutomaticRedirectAfterSignOut = true;

    // 用户凭证错误时显示的错误信息。
    public static string InvalidCredentialsErrorMessage = "Invalid username or password";
}
