namespace Microsoft.eShopOnContainers.Services.Identity.API.Models.AccountViewModels
{
    // ResetPasswordViewModel 用于重置密码时的数据传输对象
    public record ResetPasswordViewModel
    {
        // 邮箱属性，必须填写且格式必须为有效邮箱地址
        [Required] // 必填字段
        [EmailAddress] // 格式验证为邮箱地址
        public string Email { get; init; }

        // 密码属性，必须填写，长度要求6到100个字符，并且以密码格式显示
        [Required] // 必填字段
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)] // 长度限制
        [DataType(DataType.Password)] // 数据类型为密码，加密显示
        public string Password { get; init; }

        // 确认密码属性，用于验证密码输入的一致性
        [DataType(DataType.Password)] // 数据类型为密码，加密显示
        [Display(Name = "Confirm password")] // 表单显示的名称
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")] // 与 Password 属性比对
        public string ConfirmPassword { get; init; }

        // 重置密码所需的验证码
        public string Code { get; init; }
    }
}
