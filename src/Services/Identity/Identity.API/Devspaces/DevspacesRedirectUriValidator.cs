namespace Microsoft.eShopOnContainers.Services.Identity.API.Devspaces
{
    using Microsoft.Extensions.Logging;

    // 实现 IRedirectUriValidator 接口，用于验证客户端使用的重定向 URI 是否有效
    public class DevspacesRedirectUriValidator : IRedirectUriValidator
    {
        // 声明一个 ILogger 类型的字段，用于记录日志信息
        private readonly ILogger _logger;

        // 构造函数注入 ILogger 实例以便在服务中记录信息
        public DevspacesRedirectUriValidator(ILogger<DevspacesRedirectUriValidator> logger)
        {
            _logger = logger;
        }

        // 方法用于验证注销后的重定向 URI 是否有效
        // 参数 requestedUri 为客户端请求的注销后重定向 URI
        // 参数 client 为请求的客户端信息
        public Task<bool> IsPostLogoutRedirectUriValidAsync(string requestedUri, Duende.IdentityServer.Models.Client client)
        {
            // 记录日志，标记客户端使用了注销后重定向 URI
            _logger.LogInformation("Client {ClientName} used post logout uri {RequestedUri}.", client.ClientName, requestedUri);
            // 此处直接返回 true，表示该 URI 被验证为有效
            return Task.FromResult(true);
        }

        // 方法用于验证登录时的重定向 URI 是否有效
        // 参数 requestedUri 为客户端请求的重定向 URI
        // 参数 client 为请求的客户端信息
        public Task<bool> IsRedirectUriValidAsync(string requestedUri, Duende.IdentityServer.Models.Client client)
        {
            // 记录日志，标记客户端使用了重定向 URI
            _logger.LogInformation("Client {ClientName} used post logout uri {RequestedUri}.", client.ClientName, requestedUri);
            // 此处直接返回 true，表示该 URI 被验证为有效
            return Task.FromResult(true);
        }
    }
}