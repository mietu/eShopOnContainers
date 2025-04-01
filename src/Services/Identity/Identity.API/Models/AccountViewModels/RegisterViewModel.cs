namespace Microsoft.eShopOnContainers.Services.Identity.API.Models.AccountViewModels
{
    /// <summary>
    /// 注册视图模型，用于接收和验证注册时提交的数据
    /// </summary>
    public record RegisterViewModel
    {
        // 标记该属性为必填项，并且要求格式为有效Email地址
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; init; }

        // 标记为必填项
        // 限定字符串长度为最小6个字符, 最大100个字符，并设置对应的错误提示信息
        // DataType设置为Password，用于在UI上显示为密码字段（例如：隐藏输入的字符）
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; init; }

        // 数据类型指定为Password，避免在UI上显示明文
        // Display设定为“Confirm password”
        // [Compare]属性用于验证该属性值必须与Password属性值一致，否则会返回错误信息
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; init; }

        // 此属性表示注册用户的其他信息，类型为ApplicationUser，可能包含更多详细的用户资料
        public ApplicationUser User { get; init; }
    }
}
