namespace Microsoft.eShopOnContainers.Services.Identity.API.Models.AccountViewModels
{
    /// <summary>
    /// 验证码视图模型，用于在用户验证时传递数据
    /// </summary>
    public record VerifyCodeViewModel
    {
        /// <summary>
        /// 验证码提供者（例如短信、电子邮件等）。
        /// 此属性为必填项。
        /// </summary>
        [Required]
        public string Provider { get; init; }

        /// <summary>
        /// 用户输入的验证码。
        /// 此属性为必填项。
        /// </summary>
        [Required]
        public string Code { get; init; }

        /// <summary>
        /// 完成验证后需要返回的 URL 地址。
        /// </summary>
        public string ReturnUrl { get; init; }

        /// <summary>
        /// 是否记住当前浏览器，下次验证时无需重复验证。
        /// 显示名称为“Remember this browser?”。
        /// </summary>
        [Display(Name = "Remember this browser?")]
        public bool RememberBrowser { get; init; }

        /// <summary>
        /// 是否记住用户登录状态，保持会话持久化。
        /// 显示名称为“Remember me?”。
        /// </summary>
        [Display(Name = "Remember me?")]
        public bool RememberMe { get; init; }
    }
}
