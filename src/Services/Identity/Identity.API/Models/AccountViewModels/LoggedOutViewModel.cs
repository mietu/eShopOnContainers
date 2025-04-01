namespace Microsoft.eShopOnContainers.Services.Identity.API.Models.AccountViewModels
{
    // 记录类型：用于表示用户退出登录后返回的视图模型
    public record LoggedOutViewModel
    {
        // 登出后重定向的 URI
        public string PostLogoutRedirectUri { get; init; }

        // 客户端名称
        public string ClientName { get; init; }

        // 用于退出登录时嵌入的 iframe URL
        public string SignOutIframeUrl { get; init; }
    }
}