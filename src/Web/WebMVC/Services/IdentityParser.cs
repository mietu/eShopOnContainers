namespace Microsoft.eShopOnContainers.WebMVC.Services;

/// <summary>
/// 身份解析器类，实现了IIdentityParser接口，用于从用户凭据中提取用户信息
/// </summary>
public class IdentityParser : IIdentityParser<ApplicationUser>
{
    /// <summary>
    /// 从IPrincipal对象中解析出ApplicationUser对象
    /// </summary>
    /// <param name="principal">用户凭据对象</param>
    /// <returns>包含用户详细信息的ApplicationUser对象</returns>
    /// <exception cref="ArgumentException">当传入参数不是ClaimsPrincipal类型时抛出</exception>
    public ApplicationUser Parse(IPrincipal principal)
    {
        // 使用C#模式匹配'is'表达式
        // 如果principal是ClaimsPrincipal类型，则将其赋值给claims变量
        if (principal is ClaimsPrincipal claims)
        {
            return new ApplicationUser
            {
                // 从claims中提取持卡人姓名，如果不存在则返回空字符串
                CardHolderName = claims.Claims.FirstOrDefault(x => x.Type == "card_holder")?.Value ?? "",
                // 从claims中提取卡号，如果不存在则返回空字符串
                CardNumber = claims.Claims.FirstOrDefault(x => x.Type == "card_number")?.Value ?? "",
                // 从claims中提取卡有效期，如果不存在则返回空字符串
                Expiration = claims.Claims.FirstOrDefault(x => x.Type == "card_expiration")?.Value ?? "",
                // 从claims中提取卡类型，转换为整数，如果不存在则默认为0
                CardType = int.Parse(claims.Claims.FirstOrDefault(x => x.Type == "missing")?.Value ?? "0"),
                // 从claims中提取城市信息，如果不存在则返回空字符串
                City = claims.Claims.FirstOrDefault(x => x.Type == "address_city")?.Value ?? "",
                // 从claims中提取国家信息，如果不存在则返回空字符串
                Country = claims.Claims.FirstOrDefault(x => x.Type == "address_country")?.Value ?? "",
                // 从claims中提取电子邮件，如果不存在则返回空字符串
                Email = claims.Claims.FirstOrDefault(x => x.Type == "email")?.Value ?? "",
                // 从claims中提取用户ID(subject)，如果不存在则返回空字符串
                Id = claims.Claims.FirstOrDefault(x => x.Type == "sub")?.Value ?? "",
                // 从claims中提取用户姓氏，如果不存在则返回空字符串
                LastName = claims.Claims.FirstOrDefault(x => x.Type == "last_name")?.Value ?? "",
                // 从claims中提取用户名字，如果不存在则返回空字符串
                Name = claims.Claims.FirstOrDefault(x => x.Type == "name")?.Value ?? "",
                // 从claims中提取电话号码，如果不存在则返回空字符串
                PhoneNumber = claims.Claims.FirstOrDefault(x => x.Type == "phone_number")?.Value ?? "",
                // 从claims中提取信用卡安全码，如果不存在则返回空字符串
                SecurityNumber = claims.Claims.FirstOrDefault(x => x.Type == "card_security_number")?.Value ?? "",
                // 从claims中提取州/省信息，如果不存在则返回空字符串
                State = claims.Claims.FirstOrDefault(x => x.Type == "address_state")?.Value ?? "",
                // 从claims中提取街道信息，如果不存在则返回空字符串
                Street = claims.Claims.FirstOrDefault(x => x.Type == "address_street")?.Value ?? "",
                // 从claims中提取邮政编码，如果不存在则返回空字符串
                ZipCode = claims.Claims.FirstOrDefault(x => x.Type == "address_zip_code")?.Value ?? ""
            };
        }
        // 如果principal不是ClaimsPrincipal类型，则抛出参数异常
        throw new ArgumentException(message: "The principal must be a ClaimsPrincipal", paramName: nameof(principal));
    }
}
