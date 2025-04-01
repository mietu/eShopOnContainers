namespace WebMVC.ViewModels.Annotations;

/// <summary>
/// 表示一个用于验证纬度坐标的自定义验证特性
/// 纬度值必须在-90到90度之间（包含边界值）
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class LatitudeCoordinate : ValidationAttribute
{
    /// <summary>
    /// 验证指定的值是否为有效的纬度坐标
    /// </summary>
    /// <param name="value">要验证的值</param>
    /// <param name="validationContext">验证上下文</param>
    /// <returns>如果值有效则返回ValidationResult.Success，否则返回包含错误消息的ValidationResult</returns>
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        // 如果值为null，则返回成功（null值的验证应由Required特性处理）
        if (value == null)
        {
            return ValidationResult.Success;
        }

        // 尝试将值转换为double类型的纬度坐标
        double coordinate;
        // 如果转换失败或值不在有效范围内（-90到90度之间），则返回验证错误
        if (!double.TryParse(value.ToString(), out coordinate) || (coordinate < -90 || coordinate > 90))
        {
            return new ValidationResult("Latitude must be between -90 and 90 degrees inclusive.");
        }

        // 验证通过
        return ValidationResult.Success;
    }
}
