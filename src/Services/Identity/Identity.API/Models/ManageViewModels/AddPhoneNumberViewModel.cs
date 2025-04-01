namespace Microsoft.eShopOnContainers.Services.Identity.API.Models.ManageViewModels
{
    // 定义用于添加电话号码的视图模型（视图数据传输对象）
    public record AddPhoneNumberViewModel
    {
        /// <summary>
        /// 电话号码
        /// </summary>
        [Required] // 指定此字段为必须填写
        [Phone] // 确保输入值符合电话号码格式
        [Display(Name = "Phone number")] // 设置在界面上显示的字段名称
        public string PhoneNumber { get; init; }
    }
}
