namespace Microsoft.eShopOnContainers.WebMVC.ViewModels.Annotations;

/// <summary>
/// 自定义验证特性，用于验证信用卡过期日期是否有效
/// 可应用于属性或字段，并允许多次使用
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class CardExpirationAttribute : ValidationAttribute
{
    /// <summary>
    /// 验证信用卡过期日期是否有效
    /// </summary>
    /// <param name="value">待验证的日期值，预期格式为"MM/YY"</param>
    /// <returns>如果过期日期晚于当前日期则返回true，否则返回false</returns>
    public override bool IsValid(object value)
    {
        // 如果值为空，则验证失败
        if (value == null)
            return false;

        // 分割月份和年份，格式应为"MM/YY"
        var monthString = value.ToString().Split('/')[0];
        // 将两位数的年份转换为四位数（假设为21世纪）
        var yearString = $"20{value.ToString().Split('/')[1]}";

        // 尝试将月份和年份字符串转换为整数
        // 使用C#7.0引入的out变量声明简化代码
        if ((int.TryParse(monthString, out var month)) &&
            (int.TryParse(yearString, out var year)))
        {
            // 创建表示过期日期的第一天的DateTime对象
            DateTime d = new DateTime(year, month, 1);

            // 检查过期日期是否在当前日期之后
            return d > DateTime.UtcNow;
        }
        else
        {
            // 如果无法解析月份或年份，则验证失败
            return false;
        }
    }
}
