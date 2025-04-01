// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityServerHost.Quickstart.UI;

[SecurityHeaders]
[Authorize]
public class DiagnosticsController : Controller
{
    public async Task<IActionResult> Index()
    {
        // 定义允许访问的本地地址列表，包括IPv4和IPv6循环地址，以及当前服务器的本地IP地址
        var localAddresses = new string[] {
                    "127.0.0.1",
                    "::1",
                    HttpContext.Connection.LocalIpAddress.ToString()
                };

        // 如果请求的远程IP地址不在允许的本地地址列表中，则返回404错误
        if (!localAddresses.Contains(HttpContext.Connection.RemoteIpAddress.ToString()))
        {
            return NotFound();
        }

        // 异步获取认证票据信息，并利用票据创建诊断视图模型
        var model = new DiagnosticsViewModel(await HttpContext.AuthenticateAsync());

        // 返回该视图及其模型
        return View(model);
    }
}
