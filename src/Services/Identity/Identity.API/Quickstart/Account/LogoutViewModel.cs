// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityServerHost.Quickstart.UI;

// LogoutViewModel 继承自 LogoutInputModel，扩展了原有的登出模型
public class LogoutViewModel : LogoutInputModel
{
    // 表示是否需要显示登出提示，默认值为 true。
    // 当该值为 true 时，前端界面可能会要求用户确认登出操作
    public bool ShowLogoutPrompt { get; set; } = true;
}
