namespace Devspaces.Support;

// ServiceCollectionDevspacesExtensions 类：为 IServiceCollection 提供扩展方法，以便添加 Devspaces 相关的服务。
public static class ServiceCollectionDevspacesExtensions
{
    // AddDevspaces 扩展方法：将 DevspacesMessageHandler 注册为瞬态服务，并返回 IServiceCollection 实例以便支持链式调用。
    public static IServiceCollection AddDevspaces(this IServiceCollection services)
    {
        // 注册 DevspacesMessageHandler 为瞬态服务，每次请求都会创建新的实例。
        services.AddTransient<DevspacesMessageHandler>();

        // 返回 IServiceCollection 以便进行后续的链式调用。
        return services;
    }
}
