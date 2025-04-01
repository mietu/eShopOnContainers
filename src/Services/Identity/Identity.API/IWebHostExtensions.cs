namespace Microsoft.AspNetCore.Hosting
{
    /// <summary>
    /// 扩展 IWebHost 的方法，提供数据库迁移以及在 Kubernetes 环境下的判断功能
    /// </summary>
    public static class IWebHostExtensions
    {
        /// <summary>
        /// 判断当前应用是否在 Kubernetes 环境中运行
        /// 通过读取配置项 "OrchestratorType"，如果值为 "K8S" 则认为在 Kubernetes 环境中
        /// </summary>
        /// <param name="webHost">IWebHost 实例</param>
        /// <returns>如果在 Kubernetes 中返回 true，否则返回 false</returns>
        public static bool IsInKubernetes(this IWebHost webHost)
        {
            // 获取配置服务
            var cfg = webHost.Services.GetService<IConfiguration>();
            // 从配置中获取 OrchestratorType 的值
            var orchestratorType = cfg.GetValue<string>("OrchestratorType");
            // 转换为大写后比较是否为 "K8S"
            return orchestratorType?.ToUpper() == "K8S";
        }

        /// <summary>
        /// 对指定的 DbContext 进行数据库迁移，并执行自定义种子数据填充逻辑
        /// 根据是否在 Kubernetes 环境中决定是否使用重试策略
        /// </summary>
        /// <typeparam name="TContext">需要迁移的 DbContext 类型</typeparam>
        /// <param name="webHost">IWebHost 实例</param>
        /// <param name="seeder">种子数据填充方法的委托</param>
        /// <returns>返回被操作后的 IWebHost</returns>
        public static IWebHost MigrateDbContext<TContext>(this IWebHost webHost, Action<TContext, IServiceProvider> seeder) where TContext : DbContext
        {
            // 判断是否在 Kubernetes 环境中运行
            var underK8s = webHost.IsInKubernetes();

            // 创建服务作用域
            using var scope = webHost.Services.CreateScope();
            var services = scope.ServiceProvider;
            // 获取日志记录器
            var logger = services.GetRequiredService<ILogger<TContext>>();
            // 获取 DbContext 实例
            var context = services.GetService<TContext>();

            try
            {
                logger.LogInformation("Migrating database associated with context {DbContextName}", typeof(TContext).Name);

                if (underK8s)
                {
                    // 在 Kubernetes 中直接调用，不使用重试策略
                    InvokeSeeder(seeder, context, services);
                }
                else
                {
                    // 定义重试次数
                    var retries = 10;
                    // 使用 Polly 定义处理 SqlException 的重试策略
                    var retry = Policy.Handle<SqlException>()
                        .WaitAndRetry(
                            retryCount: retries,
                            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                            onRetry: (exception, timeSpan, retry, ctx) =>
                            {
                                // 重试过程中记录警告日志
                                logger.LogWarning(exception, "[{prefix}] Exception {ExceptionType} with message {Message} detected on attempt {retry} of {retries}", nameof(TContext), exception.GetType().Name, exception.Message, retry, retries);
                            });

                    // 针对 transient 异常进行重试，执行数据库迁移和初始化
                    retry.Execute(() => InvokeSeeder(seeder, context, services));
                }

                logger.LogInformation("Migrated database associated with context {DbContextName}", typeof(TContext).Name);
            }
            catch (Exception ex)
            {
                // 记录迁移过程中发生的错误
                logger.LogError(ex, "An error occurred while migrating the database used on context {DbContextName}", typeof(TContext).Name);
                if (underK8s)
                {
                    // 在 Kubernetes 中抛出异常，由 orchestrator 重新启动容器
                    throw;
                }
            }

            return webHost;
        }

        /// <summary>
        /// 执行指定的数据库迁移操作，并调用自定义的种子数据填充方法
        /// </summary>
        /// <typeparam name="TContext">DbContext 类型</typeparam>
        /// <param name="seeder">填充方法委托</param>
        /// <param name="context">DbContext 实例</param>
        /// <param name="services">服务提供者</param>
        private static void InvokeSeeder<TContext>(Action<TContext, IServiceProvider> seeder, TContext context, IServiceProvider services)
            where TContext : DbContext
        {
            // 调用 EF Core 的数据库迁移操作
            context.Database.Migrate();
            // 执行自定义的种子数据填充逻辑
            seeder(context, services);
        }
    }
}
