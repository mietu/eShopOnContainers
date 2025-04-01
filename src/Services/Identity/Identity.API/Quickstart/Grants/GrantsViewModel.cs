// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityServerHost.Quickstart.UI;

/// <summary>
/// 表示包含多个授权视图模型的容器
/// </summary>
public class GrantsViewModel
{
    // 表示授权列表，每个元素是单个客户端授权信息
    public IEnumerable<GrantViewModel> Grants { get; set; }
}

/// <summary>
/// 表示单个客户端的授权信息
/// </summary>
public class GrantViewModel
{
    // 客户端的唯一标识符
    public string ClientId { get; set; }
    // 客户端的名称
    public string ClientName { get; set; }
    // 客户端的网址
    public string ClientUrl { get; set; }
    // 客户端Logo的链接地址
    public string ClientLogoUrl { get; set; }
    // 授权的描述信息
    public string Description { get; set; }
    // 授权创建的时间
    public DateTime Created { get; set; }
    // 授权过期的时间（可为 null 表示无过期时间）
    public DateTime? Expires { get; set; }
    // 身份验证授权名称列表
    public IEnumerable<string> IdentityGrantNames { get; set; }
    // API 授权名称列表
    public IEnumerable<string> ApiGrantNames { get; set; }
}
