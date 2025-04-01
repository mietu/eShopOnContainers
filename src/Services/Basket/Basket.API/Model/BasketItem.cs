namespace Microsoft.eShopOnContainers.Services.Basket.API.Model;

// BasketItem 表示购物篮中的一个项目，并实现了 IValidatableObject 接口用于数据验证
public class BasketItem : IValidatableObject
{
    // 唯一标识符
    public string Id { get; set; }

    // 产品的唯一标识符
    public int ProductId { get; set; }

    // 产品的名称
    public string ProductName { get; set; }

    // 当前单价
    public decimal UnitPrice { get; set; }

    // 原始单价
    public decimal OldUnitPrice { get; set; }

    // 购买数量
    public int Quantity { get; set; }

    // 产品图片的 URL 地址
    public string PictureUrl { get; set; }

    // 实现 IValidatableObject 接口的方法，用于执行自定义校验逻辑
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // 创建一个列表用于存储验证结果
        var results = new List<ValidationResult>();

        // 如果购买数量小于 1，则添加错误信息
        if (Quantity < 1)
        {
            results.Add(new ValidationResult("Invalid number of units", new[] { "Quantity" }));
        }

        // 返回所有验证结果
        return results;
    }
}
