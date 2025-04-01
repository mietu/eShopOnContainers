namespace Microsoft.eShopOnContainers.Services.Identity.API.Models.ManageViewModels
{
    // ChangePasswordViewModel 记录类型，用于封装修改密码时提交的数据
    public record ChangePasswordViewModel
    {
        // OldPassword 属性：
        // - 必填字段
        // - 数据类型为密码，浏览器将隐藏输入内容
        // - 显示名称为 “Current password”
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; init; }

        // NewPassword 属性：
        // - 必填字段
        // - 字符串长度最小为6，最大为100
        // - 数据类型为密码
        // - 显示名称为 “New password”
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; init; }

        // ConfirmPassword 属性：
        // - 数据类型为密码
        // - 显示名称为 “Confirm new password”
        // - 值必须与 NewPassword 属性相匹配，验证两次输入是否一致
        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; init; }
    }
}
