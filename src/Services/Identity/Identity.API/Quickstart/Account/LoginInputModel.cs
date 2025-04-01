// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityServerHost.Quickstart.UI;

/// <summary>
/// 登录输入模型，用于封装用户登录时提交的数据
/// </summary>
public class LoginInputModel
{
    // [Required] 特性表示该属性是必填项
    [Required]
    public string Username { get; set; } // 用户名

    // [Required] 特性表示该属性是必填项
    [Required]
    public string Password { get; set; } // 密码

    // 是否记住登录状态
    public bool RememberLogin { get; set; }

    // 登录成功后返回的 URL 地址
    public string ReturnUrl { get; set; }
}
