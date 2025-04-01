namespace Microsoft.eShopOnContainers.Services.Identity.API.Models.ConsentViewModels
{
    // ConsentViewModel 类继承自 ConsentInputModel，并扩展了额外的属性，
    // 用于在用户同意授权某些范围时提供相关客户端信息和详细的范围列表。
    public class ConsentViewModel : ConsentInputModel
    {
        // 客户端名称
        public string ClientName { get; set; }

        // 客户端 URL 地址
        public string ClientUrl { get; set; }

        // 客户端 Logo 图标 URL 地址
        public string ClientLogoUrl { get; set; }

        // 是否允许记住用户的授权决策
        public bool AllowRememberConsent { get; set; }

        // 包含身份范围的列表，每个项为 ScopeViewModel 类型
        public IEnumerable<ScopeViewModel> IdentityScopes { get; set; }

        // 包含 API 范围的列表，每个项为 ScopeViewModel 类型
        public IEnumerable<ScopeViewModel> ApiScopes { get; set; }
    }
}