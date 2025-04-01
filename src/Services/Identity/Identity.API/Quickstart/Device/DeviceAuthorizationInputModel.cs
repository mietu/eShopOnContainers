// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityServerHost.Quickstart.UI;

/// <summary>
/// 表示设备授权流程中的输入数据模型，继承自 ConsentInputModel，
/// 其中包含用户代码（UserCode）用于验证设备用户的授权操作。
/// </summary>
public class DeviceAuthorizationInputModel : ConsentInputModel
{
    /// <summary>
    /// 获取或设置用户在设备授权过程中输入的用户代码。
    /// 用户代码用于唯一标识和验证设备授权请求。
    /// </summary>
    public string UserCode { get; set; }
}
