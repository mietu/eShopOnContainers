namespace Microsoft.eShopOnContainers.Services.Catalog.API.Extensions;

public static class WebHostExtensions
{
    // 判断当前运行环境是否为 Kubernetes 环境
    public static bool IsInKubernetes(this IWebHost host)
    {
        // 从依赖注入容器中获取 IConfiguration 配置对象
        var cfg = host.Services.GetService<IConfiguration>();
        // 从配置中获取 OrchestratorType 值
        var orchestratorType = cfg.GetValue<string>("OrchestratorType");
        // 判断是否为 "K8S"（不区分大小写）
        return orchestratorType?.ToUpper() == "K8S";
    }

    // 扩展方法：执行数据库上下文的迁移，并执行种子数据初始化
    public static IWebHost MigrateDbContext<TContext>(this IWebHost host, Action<TContext, IServiceProvider> seeder) where TContext : DbContext
    {
        // 判断是否在 Kubernetes 环境下运行
        var underK8s = host.IsInKubernetes();

        // 为 DbContext 创建一个作用域
        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;

        // 从 DI 容器中获取指定类型的日志记录器
        var logger = services.GetRequiredService<ILogger<TContext>>();

        // 从容器中获取 DbContext 实例
        var context = services.GetService<TContext>();

        try
        {
            // 记录数据库迁移开始的日志信息
            logger.LogInformation("Migrating database associated with context {DbContextName}", typeof(TContext).Name);

            if (underK8s)
            {
                // 在 Kubernetes 环境下，直接调用种子数据初始化方法
                InvokeSeeder(seeder, context, services);
            }
            else
            {
                // 在非 Kubernetes 环境下，使用 Polly 重试策略应对 SQL 异常
                var retry = Policy.Handle<SqlException>()
                        .WaitAndRetry(new TimeSpan[]
                        {
                                TimeSpan.FromSeconds(3),
                                TimeSpan.FromSeconds(5),
                                TimeSpan.FromSeconds(8),
                        });

                // 对迁移操作应用重试策略
                retry.Execute(() => InvokeSeeder(seeder, context, services));
            }

            // 记录数据库迁移完成的日志信息
            logger.LogInformation("Migrated database associated with context {DbContextName}", typeof(TContext).Name);
        }
        catch (Exception ex)
        {
            // 如果迁移过程中发生异常，则记录错误日志
            logger.LogError(ex, "An error occurred while migrating the database used on context {DbContextName}", typeof(TContext).Name);
            // 如果在 Kubernetes 环境，则抛出异常，依赖 Kubernetes 重启 Pod
            if (underK8s)
            {
                throw;
            }
        }

        return host;
    }

    // 私有方法：执行数据库迁移和种子数据初始化
    private static void InvokeSeeder<TContext>(Action<TContext, IServiceProvider> seeder, TContext context, IServiceProvider services)
        where TContext : DbContext
    {
        // 进行数据库迁移（更新数据库到最新状态）
        context.Database.Migrate();
        // 调用委托方法进行种子数据初始化
        seeder(context, services);
    }
}
