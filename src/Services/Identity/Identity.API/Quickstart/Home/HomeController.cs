// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityServerHost.Quickstart.UI
{
    // 添加安全相关响应头
    [SecurityHeaders]
    // 允许匿名访问该控制器
    [AllowAnonymous]
    public class HomeController : Controller
    {
        // 用于与IdentityServer交互, 获取错误详情信息等
        private readonly IIdentityServerInteractionService _interaction;
        // 用于获取当前Web应用运行的环境信息(如开发或生产环境)
        private readonly IWebHostEnvironment _environment;
        // 日志记录
        private readonly ILogger _logger;

        // 构造函数, 注入依赖项
        public HomeController(
            IIdentityServerInteractionService interaction,
            IWebHostEnvironment environment,
            ILogger<HomeController> logger)
        {
            _interaction = interaction;
            _environment = environment;
            _logger = logger;
        }

        // GET: 首页请求处理方法
        public IActionResult Index()
        {
            // 如果环境为开发环境，则直接返回视图
            if (_environment.IsDevelopment())
            {
                // 仅在开发环境下显示主页
                return View();
            }

            // 如果不是开发环境，记录日志并返回404以隐藏主页
            _logger.LogInformation("Homepage is disabled in production. Returning 404.");
            return NotFound();
        }

        /// <summary>
        /// 错误处理方法, 显示错误页面
        /// </summary>
        public async Task<IActionResult> Error(string errorId)
        {
            // 创建错误视图模型
            var vm = new ErrorViewModel();

            // 从IdentityServer获取错误详情, 传入错误ID
            var message = await _interaction.GetErrorContextAsync(errorId);
            if (message != null)
            {
                // 将错误详情赋值给视图模型
                vm.Error = message;

                // 非开发环境中, 不显示详细错误描述
                if (!_environment.IsDevelopment())
                {
                    message.ErrorDescription = null;
                }
            }

            // 返回错误视图页面, 并传入错误视图模型
            return View("Error", vm);
        }
    }
}