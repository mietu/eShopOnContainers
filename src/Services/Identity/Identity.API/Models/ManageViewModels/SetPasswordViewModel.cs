namespace Microsoft.eShopOnContainers.Services.Identity.API.Models.ManageViewModels
{
    // 此记录类型用于设置新密码的视图模型
    public record SetPasswordViewModel
    {
        // 标记 NewPassword 字段为必填字段
        [Required]
        // 限制字符串长度：最小6字符，最大100字符
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        // 指定此字段为密码类型（在UI中可隐藏输入内容）
        [DataType(DataType.Password)]
        // 指定UI显示时的标签名称
        [Display(Name = "New password")]
        public string NewPassword { get; init; }

        // 指定此字段为密码类型
        [DataType(DataType.Password)]
        // 指定UI显示时的标签名称
        [Display(Name = "Confirm new password")]
        // 确保 ConfirmPassword 与 NewPassword 字段的值匹配
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; init; }
    }
}
