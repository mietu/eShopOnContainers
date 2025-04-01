namespace Microsoft.eShopOnContainers.Services.Identity.API.Models.AccountViewModels
{
    // 登录视图模型，用于用户登录时提交表单数据
    public record LoginViewModel
    {
        // 指示此字段为必填项
        [Required]
        // 指定该字段的数据应为电子邮件格式
        [EmailAddress]
        public string Email { get; set; } // 用户的电子邮箱地址

        // 指示此字段为必填项
        [Required]
        // 指定该字段的数据为密码格式
        [DataType(DataType.Password)]
        public string Password { get; set; } // 用户登录密码

        // 为属性设置显示名称，用户界面上显示为“Remember me?”
        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; } // 是否记住用户登录状态

        // 登录后跳转的URL
        public string ReturnUrl { get; set; } // 登录成功后返回的地址
    }
}