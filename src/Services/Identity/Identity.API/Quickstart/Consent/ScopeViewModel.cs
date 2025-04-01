// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityServerHost.Quickstart.UI;

// ScopeViewModel 类用于表示 OAuth2/OpenID Connect 中的 scope（权限范围）
public class ScopeViewModel
{
    // Value: 表示 scope 的唯一标识符，用于内部逻辑或与客户端对接时的标识
    public string Value { get; set; }

    // DisplayName: 为用户展示的 scope 名称，便于用户识别
    public string DisplayName { get; set; }

    // Description: 提供该 scope 的详细描述，帮助用户理解其作用
    public string Description { get; set; }

    // Emphasize: 指示是否在 UI 中强调显示该 scope，通常使用高亮或其他视觉手段
    public bool Emphasize { get; set; }

    // Required: 指示该 scope 是否为必选项，用户不能将其取消
    public bool Required { get; set; }

    // Checked: 默认是否选中该 scope，可以在 UI 初始化时设置
    public bool Checked { get; set; }
}
