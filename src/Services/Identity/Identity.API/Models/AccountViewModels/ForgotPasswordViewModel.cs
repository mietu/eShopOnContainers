namespace Microsoft.eShopOnContainers.Services.Identity.API.Models.AccountViewModels
{
    // 定义一个不可变的记录类型 ForgotPasswordViewModel，用于表示忘记密码的视图模型
    public record ForgotPasswordViewModel
    {
        // 此属性代表用户的电子邮件地址
        // [Required] 指定该属性为必填项
        // [EmailAddress] 验证输入值是否为合法的电子邮件地址格式
        [Required]
        [EmailAddress]
        public string Email { get; init; }
    }
}
