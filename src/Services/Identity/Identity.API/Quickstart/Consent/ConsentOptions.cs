// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityServerHost.Quickstart.UI;

/// <summary>
/// ConsentOptions 类定义了与同意页面相关的选项和错误信息。
/// 这些选项用于在用户授权时显示相关信息。
/// </summary>
public class ConsentOptions
{
    // 是否启用离线访问功能。当启用时，用户可以在离线状态下授权应用访问相关资源。
    public static bool EnableOfflineAccess = true;

    // 当启用离线访问时显示的名称。
    public static string OfflineAccessDisplayName = "Offline Access";

    // 离线访问的描述信息，向用户解释其用途。
    public static string OfflineAccessDescription = "Access to your applications and resources, even when you are offline";

    // 错误信息：用户必须至少选择一个权限。
    public static readonly string MustChooseOneErrorMessage = "You must pick at least one permission";

    // 错误信息：用户选择了无效的权限。
    public static readonly string InvalidSelectionErrorMessage = "Invalid selection";
}
