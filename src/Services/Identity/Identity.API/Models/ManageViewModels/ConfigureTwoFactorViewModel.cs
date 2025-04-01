namespace Microsoft.eShopOnContainers.Services.Identity.API.Models.ManageViewModels
{
    // 定义一个 record 类型，用于封装配置双因素认证时所需的数据
    public record ConfigureTwoFactorViewModel
    {
        // 表示用户选择的双因素认证提供者，例如 SMS 或者基于应用的验证
        public string SelectedProvider { get; init; }

        // 表示可用的认证提供者列表，通常用于在前端页面中生成下拉选择框
        public ICollection<SelectListItem> Providers { get; init; }
    }
}
