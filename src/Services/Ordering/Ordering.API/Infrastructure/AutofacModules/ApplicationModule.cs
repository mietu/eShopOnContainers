namespace Microsoft.eShopOnContainers.Services.Ordering.API.Infrastructure.AutofacModules;

public class ApplicationModule : Autofac.Module
{
    // 连接字符串，用于查询数据库
    public string QueriesConnectionString { get; }

    // 构造函数，初始化连接字符串
    public ApplicationModule(string qconstr)
    {
        QueriesConnectionString = qconstr;
    }

    // 重写Load方法，注册依赖项
    protected override void Load(ContainerBuilder builder)
    {
        // 注册OrderQueries类，使用QueriesConnectionString初始化，并作为IOrderQueries接口的实现
        builder.Register(c => new OrderQueries(QueriesConnectionString))
            .As<IOrderQueries>()
            .InstancePerLifetimeScope();

        // 注册BuyerRepository类，作为IBuyerRepository接口的实现
        builder.RegisterType<BuyerRepository>()
            .As<IBuyerRepository>()
            .InstancePerLifetimeScope();

        // 注册OrderRepository类，作为IOrderRepository接口的实现
        builder.RegisterType<OrderRepository>()
            .As<IOrderRepository>()
            .InstancePerLifetimeScope();

        // 注册RequestManager类，作为IRequestManager接口的实现
        builder.RegisterType<RequestManager>()
            .As<IRequestManager>()
            .InstancePerLifetimeScope();

        // 注册程序集中的所有IIntegrationEventHandler<>接口的实现类
        builder.RegisterAssemblyTypes(typeof(CreateOrderCommandHandler).GetTypeInfo().Assembly)
            .AsClosedTypesOf(typeof(IIntegrationEventHandler<>));
    }
}
