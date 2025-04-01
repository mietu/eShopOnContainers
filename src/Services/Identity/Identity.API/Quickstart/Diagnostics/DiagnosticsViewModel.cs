// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Text;
using System.Text.Json;

namespace IdentityServerHost.Quickstart.UI;

// DiagnosticsViewModel 用于存储和展示身份验证相关的诊断信息
public class DiagnosticsViewModel
{
    // 构造函数，初始化 DiagnosticsViewModel
    public DiagnosticsViewModel(AuthenticateResult result)
    {
        // 将传入的 AuthenticateResult 实例保存到属性中
        AuthenticateResult = result;

        // 检查身份验证结果的属性中是否包含键 "client_list"
        if (result.Properties.Items.ContainsKey("client_list"))
        {
            // 获取编码后的客户端列表字符串，该字符串采用 Base64Url 编码
            var encoded = result.Properties.Items["client_list"];

            // 解码获取原始的字节数组
            var bytes = Base64Url.Decode(encoded);

            // 将字节数组转换成 UTF-8 字符串
            var value = Encoding.UTF8.GetString(bytes);

            // 使用 JsonSerializer 将 JSON 格式的字符串反序列化为字符串数组
            Clients = JsonSerializer.Deserialize<string[]>(value);
        }
    }

    // 只读属性，存储身份验证结果
    public AuthenticateResult AuthenticateResult { get; }

    // 只读属性，存储客户端列表，默认初始化为空列表
    public IEnumerable<string> Clients { get; } = new List<string>();
}
