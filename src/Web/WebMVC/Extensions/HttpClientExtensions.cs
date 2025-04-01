namespace Microsoft.eShopOnContainers.WebMVC.Extensions;

/// <summary>
/// 提供HttpClient的扩展方法，简化身份验证头部的设置
/// </summary>
public static class HttpClientExtensions
{
    /// <summary>
    /// 为HttpClient设置基本身份验证(Basic Authentication)头部
    /// </summary>
    /// <param name="client">要扩展的HttpClient实例</param>
    /// <param name="userName">用户名</param>
    /// <param name="password">密码</param>
    public static void SetBasicAuthentication(this HttpClient client, string userName, string password) =>
        client.DefaultRequestHeaders.Authorization = new BasicAuthenticationHeaderValue(userName, password);

    /// <summary>
    /// 为HttpClient设置指定方案的身份验证令牌
    /// </summary>
    /// <param name="client">要扩展的HttpClient实例</param>
    /// <param name="scheme">身份验证方案，如Bearer, Basic等</param>
    /// <param name="token">身份验证令牌</param>
    public static void SetToken(this HttpClient client, string scheme, string token) =>
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme, token);

    /// <summary>
    /// 为HttpClient设置Bearer类型的令牌，这是OAuth 2.0和JWT中最常用的方案
    /// </summary>
    /// <param name="client">要扩展的HttpClient实例</param>
    /// <param name="token">JWT令牌字符串</param>
    public static void SetBearerToken(this HttpClient client, string token) =>
        client.SetToken(JwtConstants.TokenType, token);
}

/// <summary>
/// 实现Basic Authentication的认证头值
/// 继承自AuthenticationHeaderValue，专门用于Basic认证方案
/// </summary>
public class BasicAuthenticationHeaderValue : AuthenticationHeaderValue
{
    /// <summary>
    /// 创建Basic认证头值
    /// </summary>
    /// <param name="userName">用户名</param>
    /// <param name="password">密码</param>
    public BasicAuthenticationHeaderValue(string userName, string password)
        : base("Basic", EncodeCredential(userName, password))
    { }

    /// <summary>
    /// 将用户名和密码编码为Basic认证所需的格式
    /// 格式为：Base64("username:password")，使用iso-8859-1编码
    /// </summary>
    /// <param name="userName">用户名</param>
    /// <param name="password">密码</param>
    /// <returns>Base64编码后的凭证字符串</returns>
    private static string EncodeCredential(string userName, string password)
    {
        // Basic认证规范要求使用iso-8859-1编码
        Encoding encoding = Encoding.GetEncoding("iso-8859-1");
        // 按照"username:password"格式组合凭证
        string credential = String.Format("{0}:{1}", userName, password);

        // 将凭证转换为Base64字符串
        return Convert.ToBase64String(encoding.GetBytes(credential));
    }
}
