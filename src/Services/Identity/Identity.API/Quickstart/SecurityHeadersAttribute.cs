// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityServerHost.Quickstart.UI;

// SecurityHeadersAttribute 用于为返回的 ViewResult 添加安全相关的 HTTP 响应头
public class SecurityHeadersAttribute : ActionFilterAttribute
{
    public override void OnResultExecuting(ResultExecutingContext context)
    {
        var result = context.Result;
        // 判断当前返回结果是否为 ViewResult 类型
        if (result is ViewResult)
        {
            // 添加 X-Content-Type-Options 响应头，防止 MIME 类型混淆攻击
            // 参考：https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-Content-Type-Options
            if (!context.HttpContext.Response.Headers.ContainsKey("X-Content-Type-Options"))
            {
                context.HttpContext.Response.Headers.Add("X-Content-Type-Options", "nosniff");
            }

            // 添加 X-Frame-Options 响应头，防止 Clickjacking 攻击
            // 参考：https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-Frame-Options
            if (!context.HttpContext.Response.Headers.ContainsKey("X-Frame-Options"))
            {
                context.HttpContext.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");
            }

            // 定义 Content-Security-Policy 策略，限制允许加载的资源
            // 参考：https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Content-Security-Policy
            var csp = "default-src 'self'; object-src 'none'; frame-ancestors 'none'; sandbox allow-forms allow-same-origin allow-scripts; base-uri 'self';";
            // 可进一步考虑在生产环境开启 HTTPS 后加入 upgrade-insecure-requests 选项
            //csp += "upgrade-insecure-requests;";
            // 也可添加允许从指定第三方加载图片资源的规则
            // csp += "img-src 'self' https://pbs.twimg.com;";

            // 为标准兼容浏览器添加 Content-Security-Policy 响应头
            if (!context.HttpContext.Response.Headers.ContainsKey("Content-Security-Policy"))
            {
                context.HttpContext.Response.Headers.Add("Content-Security-Policy", csp);
            }
            // 为 Internet Explorer 添加 X-Content-Security-Policy 响应头
            if (!context.HttpContext.Response.Headers.ContainsKey("X-Content-Security-Policy"))
            {
                context.HttpContext.Response.Headers.Add("X-Content-Security-Policy", csp);
            }

            // 添加 Referrer-Policy 响应头，设置不发送 referrer 信息
            // 参考：https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Referrer-Policy
            var referrer_policy = "no-referrer";
            if (!context.HttpContext.Response.Headers.ContainsKey("Referrer-Policy"))
            {
                context.HttpContext.Response.Headers.Add("Referrer-Policy", referrer_policy);
            }
        }
    }
}
