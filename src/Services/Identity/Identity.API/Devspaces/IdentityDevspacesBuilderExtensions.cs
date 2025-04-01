namespace Microsoft.eShopOnContainers.Services.Identity.API.Devspaces
{
    // 静态类，用于扩展 IIdentityServerBuilder 接口，添加 Devspaces 相关配置
    static class IdentityDevspacesBuilderExtensions
    {
        /// <summary>
        /// 如果需要，则添加 Devspaces 重定向 URI 验证器
        /// </summary>
        /// <param name="builder">当前的 IIdentityServerBuilder 实例</param>
        /// <param name="useDevspaces">指示是否使用 Devspaces 配置</param>
        /// <returns>返回配置后的 IIdentityServerBuilder 实例</returns>
        public static IIdentityServerBuilder AddDevspacesIfNeeded(this IIdentityServerBuilder builder, bool useDevspaces)
        {
            // 如果 useDevspaces 为 true，则添加 DevspacesRedirectUriValidator 到验证器列表
            if (useDevspaces)
            {
                builder.AddRedirectUriValidator<DevspacesRedirectUriValidator>();
            }
            // 返回修改后的 IIdentityServerBuilder 实例
            return builder;
        }
    }
}
