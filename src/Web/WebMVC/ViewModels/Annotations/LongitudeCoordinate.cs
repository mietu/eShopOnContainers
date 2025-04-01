namespace WebMVC.ViewModels.Annotations;

/// <summary>
/// 自定义验证特性，用于验证经度坐标是否在有效范围内（-180 到 180 度之间）
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class LongitudeCoordinate : ValidationAttribute
{
    /// <summary>
    /// 验证经度坐标值是否有效
    /// </summary>
    /// <param name="value">要验证的值</param>
    /// <param name="validationContext">验证上下文</param>
    /// <returns>
    /// 如果值在有效范围内（-180 到 180 度之间），则返回 ValidationResult.Success；
    /// 否则返回包含错误消息的 ValidationResult
    /// </returns>
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        double coordinate;
        // 尝试将值转换为 double 类型，并检查是否在有效范围内
        if (!double.TryParse(value.ToString(), out coordinate) || (coordinate < -180 || coordinate > 180))
        {
            return new ValidationResult("Longitude must be between -180 and 180 degrees inclusive.");
        }

        return ValidationResult.Success;
    }
}
