namespace Microsoft.eShopOnContainers.Services.Identity.API.Models.ConsentViewModels
{
    /// <summary>
    /// 表示用户同意授权时使用的选项配置
    /// </summary>
    public class ConsentOptions
    {
        // 是否允许离线访问。若为 true，则授权过程中将显示离线访问权限。
        public static bool EnableOfflineAccess = true;

        // 离线访问权限的显示名称，用于在 UI 上显示给用户
        public static string OfflineAccessDisplayName = "Offline Access";

        // 离线访问权限的描述信息，用于提示用户离线访问权限的作用
        public static string OfflineAccessDescription = "Access to your applications and resources, even when you are offline";

        // 表示用户必须至少选择一种权限的错误信息，用于表单验证
        public static readonly string MustChooseOneErrorMessage = "You must pick at least one permission";

        // 当用户选择了无效的权限时对应的错误提示信息
        public static readonly string InvalidSelectionErrorMessage = "Invalid selection";
    }
}