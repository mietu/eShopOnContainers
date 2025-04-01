// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityServerHost.Quickstart.UI;

// 定义 ConsentInputModel 类，用于处理用户授权输入数据
public class ConsentInputModel
{
    // Button 属性：表示用户点击的按钮名称（例如“同意”或“拒绝”）
    public string Button { get; set; }

    // ScopesConsented 属性：用于存储用户同意的权限范围列表
    public IEnumerable<string> ScopesConsented { get; set; }

    // RememberConsent 属性：指示是否记住用户的授权决定
    public bool RememberConsent { get; set; }

    // ReturnUrl 属性：保存授权完成后重定向的 URL
    public string ReturnUrl { get; set; }

    // Description 属性：附加的描述信息或备注
    public string Description { get; set; }
}
