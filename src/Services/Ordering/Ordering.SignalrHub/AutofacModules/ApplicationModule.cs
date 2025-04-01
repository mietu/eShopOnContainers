namespace Microsoft.eShopOnContainers.Services.Ordering.SignalrHub.AutofacModules;

/// <summary>
/// ApplicationModule 类负责注册与应用程序集相关的依赖项。
/// 此模块使用 Autofac 依赖注入容器进行配置，特别是注册实现了 IIntegrationEventHandler<T> 接口的所有处理程序。
/// </summary>
public class ApplicationModule : Autofac.Module
{
    // 查询连接字符串，用来连接数据源，当前未初始化，可以根据需求设置或扩展
    public string QueriesConnectionString { get; }

    // 默认构造函数
    public ApplicationModule()
    {
    }

    /// <summary>
    /// 重写 Autofac.Module 的 Load 方法，进行类型注册操作。
    /// </summary>
    /// <param name="builder">Autofac 的 ContainerBuilder 对象</param>
    protected override void Load(ContainerBuilder builder)
    {
        // 通过反射获取包含 OrderStatusChangedToAwaitingValidationIntegrationEvent 类型的程序集
        // 并注册该程序集中的所有闭合类型 (具体类型) 实现了 IIntegrationEventHandler<T> 接口的处理程序
        builder.RegisterAssemblyTypes(typeof(OrderStatusChangedToAwaitingValidationIntegrationEvent).GetTypeInfo().Assembly)
            .AsClosedTypesOf(typeof(IIntegrationEventHandler<>));
    }
}
