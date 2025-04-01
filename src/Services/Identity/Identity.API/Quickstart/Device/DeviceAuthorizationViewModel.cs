// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityServerHost.Quickstart.UI;

public class DeviceAuthorizationViewModel : ConsentViewModel
{
    // 用户设备授权时的用户代码，用于在设备上显示或输入
    public string UserCode { get; set; }

    // 指示用户是否确认输入或显示的用户代码
    public bool ConfirmUserCode { get; set; }
}
