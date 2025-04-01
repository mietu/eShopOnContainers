// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityServerHost.Quickstart.UI;

/// <summary>
/// 处理用户同意结果的模型类
/// </summary>
public class ProcessConsentResult
{
    // 若 RedirectUri 不为 null，则表示需要重定向
    public bool IsRedirect => RedirectUri != null;

    // 重定向目标 URL
    public string RedirectUri { get; set; }

    // 与该同意请求相关的客户端信息
    public Client Client { get; set; }

    // 若 ViewModel 不为 null，则表示需要展示同意视图
    public bool ShowView => ViewModel != null;

    // 用于传递显示同意页面所需的数据模型
    public ConsentViewModel ViewModel { get; set; }

    // 若 ValidationError 不为 null，则表示存在验证错误
    public bool HasValidationError => ValidationError != null;

    // 存储验证错误信息
    public string ValidationError { get; set; }
}
