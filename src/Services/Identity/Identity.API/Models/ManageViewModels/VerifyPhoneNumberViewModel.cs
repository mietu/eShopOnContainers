namespace Microsoft.eShopOnContainers.Services.Identity.API.Models.ManageViewModels
{
    // VerifyPhoneNumberViewModel: 用于验证用户手机号码的视图模型记录
    public record VerifyPhoneNumberViewModel
    {
        // Code属性：用户输入的验证码；必填
        [Required]
        public string Code { get; init; }

        // PhoneNumber属性：用户的手机号码；必填，必须符合手机号码格式，并且显示名称为 "Phone number"
        [Required]
        [Phone]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; init; }
    }
}
