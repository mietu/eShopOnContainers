// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityServerHost.Quickstart.UI;

/// <summary>
/// 该类用于封装登出操作所需的数据模型。
/// 当用户请求注销时，可通过该模型传递注销请求标识符，
/// 用于跨重定向或验证注销请求的合法性。
/// </summary>
public class LogoutInputModel
{
    /// <summary>
    /// 登出请求的标识符。
    /// 该标识符帮助服务端在处理注销请求时进行验证和追踪。
    /// </summary>
    public string LogoutId { get; set; }
}
