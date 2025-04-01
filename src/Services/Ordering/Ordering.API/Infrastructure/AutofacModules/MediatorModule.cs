namespace Microsoft.eShopOnContainers.Services.Ordering.API.Infrastructure.AutofacModules;

// MediatorModule 继承自 Autofac.Module，用于注册 MediatR 相关的服务
public class MediatorModule : Autofac.Module
{
    // 重写 Load 方法，配置依赖注入
    protected override void Load(ContainerBuilder builder)
    {
        // 注册 MediatR 的服务
        builder.RegisterAssemblyTypes(typeof(IMediator).GetTypeInfo().Assembly)
            .AsImplementedInterfaces();

        // 注册所有实现 IRequestHandler 接口的 Command 类
        // 这些类位于 CreateOrderCommand 所在的程序集
        builder.RegisterAssemblyTypes(typeof(CreateOrderCommand).GetTypeInfo().Assembly)
            .AsClosedTypesOf(typeof(IRequestHandler<,>));

        // 注册所有实现 INotificationHandler 接口的 DomainEventHandler 类
        // 这些类位于 ValidateOrAddBuyerAggregateWhenOrderStartedDomainEventHandler 所在的程序集
        builder.RegisterAssemblyTypes(typeof(ValidateOrAddBuyerAggregateWhenOrderStartedDomainEventHandler).GetTypeInfo().Assembly)
            .AsClosedTypesOf(typeof(INotificationHandler<>));

        // 注册所有基于 FluentValidation 库的 Command 验证器
        // 这些类位于 CreateOrderCommandValidator 所在的程序集
        builder
            .RegisterAssemblyTypes(typeof(CreateOrderCommandValidator).GetTypeInfo().Assembly)
            .Where(t => t.IsClosedTypeOf(typeof(IValidator<>)))
            .AsImplementedInterfaces();

        // 注册 ServiceFactory，用于解析依赖
        builder.Register<ServiceFactory>(context =>
        {
            var componentContext = context.Resolve<IComponentContext>();
            return t => { object o; return componentContext.TryResolve(t, out o) ? o : null; };
        });

        // 注册 MediatR 的管道行为（Pipeline Behaviors）
        builder.RegisterGeneric(typeof(LoggingBehavior<,>)).As(typeof(IPipelineBehavior<,>));
        builder.RegisterGeneric(typeof(ValidatorBehavior<,>)).As(typeof(IPipelineBehavior<,>));
        builder.RegisterGeneric(typeof(TransactionBehaviour<,>)).As(typeof(IPipelineBehavior<,>));
    }
}
