namespace Microsoft.eShopOnContainers.Services.Identity.API.Services
{
    public class RedirectService : IRedirectService
    {
        /// <summary>
        /// 从返回的 URL 中提取重定向 URI
        /// </summary>
        /// <param name="url">原始返回 URL</param>
        /// <returns>提取的重定向 URI，如果无法提取则返回空字符串</returns>
        public string ExtractRedirectUriFromReturnUrl(string url)
        {
            // HTML解码，转换类似 &amp; 之类的编码字符为正常字符
            var decodedUrl = System.Net.WebUtility.HtmlDecode(url);

            // 根据 "redirect_uri=" 分割解码后的 URL，提取可能包含重定向 URI 的部分
            var results = Regex.Split(decodedUrl, "redirect_uri=");
            if (results.Length < 2)
                return ""; // 如果没有找到 "redirect_uri=" 部分，则返回空字符串

            // 取 "redirect_uri=" 后面的一部分作为结果开始部分
            string result = results[1];

            // 根据结果中的内容判断分割关键字，如果包含 "signin-oidc" 则使用它，否则使用 "scope"
            string splitKey;
            if (result.Contains("signin-oidc"))
                splitKey = "signin-oidc";
            else
                splitKey = "scope";

            // 根据选定的分割关键字进行再次分割，提取正确的重定向 URI 部分
            results = Regex.Split(result, splitKey);
            if (results.Length < 2)
                return ""; // 如果分割后结果不包含重定向 URI，则返回空字符串

            // 取分割结果的第一个部分，该部分为实际的重定向 URI
            result = results[0];

            // 将 URL 编码的特殊字符进行还原，替换编码的冒号和斜杠，同时去除多余的 "&"
            return result.Replace("%3A", ":").Replace("%2F", "/").Replace("&", "");
        }
    }
}
