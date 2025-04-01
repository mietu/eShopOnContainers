namespace WebMVC.Services.ModelDTOs;

/// <summary>
/// 表示订单的数据传输对象
/// </summary>
/// <remarks>
/// 使用 C# record 类型实现，提供值相等性比较和不可变性
/// </remarks>
public record OrderDTO
{
    /// <summary>
    /// 获取或设置订单编号
    /// </summary>
    /// <remarks>
    /// 该属性为必填项，不能为空
    /// 使用 init 限定符使其在对象创建后不可修改
    /// </remarks>
    [Required(ErrorMessage = "订单编号不能为空")]
    public string OrderNumber { get; init; } = string.Empty;
}
