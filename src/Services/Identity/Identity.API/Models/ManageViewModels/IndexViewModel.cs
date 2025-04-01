namespace Microsoft.eShopOnContainers.Services.Identity.API.Models.ManageViewModels
{
    /// <summary>
    /// 这是用于管理用户账户信息的视图模型。
    /// </summary>
    public record IndexViewModel
    {
        // 标识用户是否设置了密码
        public bool HasPassword { get; init; }

        // 用户所关联的登录信息列表
        public IList<UserLoginInfo> Logins { get; init; }

        // 用户的电话号码
        public string PhoneNumber { get; init; }

        // 标识用户是否启用了双因素认证
        public bool TwoFactor { get; init; }

        // 指示浏览器是否记住用户登录状态
        public bool BrowserRemembered { get; init; }
    }
}
