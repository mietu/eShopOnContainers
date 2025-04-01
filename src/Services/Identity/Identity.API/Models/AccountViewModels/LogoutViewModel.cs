namespace Microsoft.eShopOnContainers.Services.Identity.API.Models.AccountViewModels
{
    /// <summary>
    /// 表示注销视图模型，用于在注销时传递注销标识符。
    /// </summary>
    public record LogoutViewModel
    {
        /// <summary>
        /// 获取或设置注销标识符，用于跟踪注销请求。
        /// </summary>
        public string LogoutId { get; set; }
    }
}