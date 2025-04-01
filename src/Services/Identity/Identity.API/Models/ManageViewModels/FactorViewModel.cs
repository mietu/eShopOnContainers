namespace Microsoft.eShopOnContainers.Services.Identity.API.Models.ManageViewModels
{
    // 使用 record 定义不可变的数据模型
    // FactorViewModel 可能用于多因素认证时传递目的或描述信息
    public record FactorViewModel
    {
        // 表示该视图模型的用途
        // init 表示该属性在对象初始化阶段可以设置，但之后不可修改
        public string Purpose { get; init; }
    }
}
