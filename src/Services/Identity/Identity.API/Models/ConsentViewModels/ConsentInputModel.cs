namespace Microsoft.eShopOnContainers.Services.Identity.API.Models.ConsentViewModels
{
    /// <summary>
    /// 用户同意模型，用于接收用户在同意页面提交的数据。
    /// </summary>
    public class ConsentInputModel
    {
        /// <summary>
        /// 用户点击的按钮名称，例如“确认”或“取消”
        /// </summary>
        public string Button { get; set; }

        /// <summary>
        /// 用户同意的权限范围列表。每个字符串代表一个被同意的范围
        /// </summary>
        public IEnumerable<string> ScopesConsented { get; set; }

        /// <summary>
        /// 指示是否记住用户的同意选择，便于后续操作自动应用该选择
        /// </summary>
        public bool RememberConsent { get; set; }

        /// <summary>
        /// 返回的URL，用于在操作完成后重定向用户
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        /// 额外的描述或附加信息，通常用于补充说明用户授权的详情
        /// </summary>
        public string Description { get; set; }
    }
}