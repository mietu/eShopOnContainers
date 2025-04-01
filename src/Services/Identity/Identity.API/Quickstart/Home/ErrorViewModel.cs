// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityServerHost.Quickstart.UI;

// ErrorViewModel 类用于封装错误信息
public class ErrorViewModel
{
    // 默认构造函数，不做任何初始化操作
    public ErrorViewModel()
    {
    }

    // 构造函数，接收一个错误字符串参数
    // 使用该参数初始化 Error 属性
    public ErrorViewModel(string error)
    {
        // 创建一个新的 ErrorMessage 对象, 并将错误字符串赋值给它的 Error 属性
        Error = new ErrorMessage { Error = error };
    }

    // Error 属性, 用于存储错误信息. 类型为 ErrorMessage, 可能包含更多关于错误的详细信息
    public ErrorMessage Error { get; set; }
}
