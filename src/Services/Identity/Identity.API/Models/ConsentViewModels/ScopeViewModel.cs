namespace Microsoft.eShopOnContainers.Services.Identity.API.Models.ConsentViewModels
{
    /// <summary>
    /// 表示用户在同意页面中展示的权限范围模型
    /// </summary>
    public class ScopeViewModel
    {
        /// <summary>
        /// 权限范围的标识值
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 显示给用户的权限名称
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// 权限的详细描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 指示是否需要强调显示此权限
        /// </summary>
        public bool Emphasize { get; set; }

        /// <summary>
        /// 指示此权限是否为必选项
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// 指示初始是否勾选此权限
        /// </summary>
        public bool Checked { get; set; }
    }
}