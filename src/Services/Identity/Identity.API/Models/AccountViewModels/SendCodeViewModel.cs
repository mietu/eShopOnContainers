namespace Microsoft.eShopOnContainers.Services.Identity.API.Models.AccountViewModels
{
    /// <summary>
    /// 表示发送验证码视图模型，用于多因素认证流程中选择验证码发送方式。
    /// </summary>
    public record SendCodeViewModel
    {
        /// <summary>
        /// 用户选择的验证码发送提供者的名称。例如："电子邮件"或"短信"。
        /// </summary>
        public string SelectedProvider { get; init; }

        /// <summary>
        /// 可用的验证码发送提供者集合。每个项通常包含显示名称和对应值。
        /// </summary>
        public ICollection<SelectListItem> Providers { get; init; }

        /// <summary>
        /// 登录完成后返回的 URL，用于重定向用户到原始页面。
        /// </summary>
        public string ReturnUrl { get; init; }

        /// <summary>
        /// 指示是否在登录时保持用户登录状态。用于实现记住我功能。
        /// </summary>
        public bool RememberMe { get; init; }
    }
}
