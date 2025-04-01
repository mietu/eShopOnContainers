// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
namespace Microsoft.eShopOnContainers.Services.Identity.API.Models
{
    // 定义一个不可变的记录类型 ErrorViewModel，
    // 用于封装错误相关的数据，便于在应用程序中传递错误信息。
    public record ErrorViewModel
    {
        // Error属性用于存储错误信息，类型为ErrorMessage。
        // 这里使用了属性初始化器，默认情况下该属性可以进行get和set操作。
        public ErrorMessage Error { get; set; }
    }
}