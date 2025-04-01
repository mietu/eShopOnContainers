using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Data.SqlClient;

namespace Microsoft.AspNetCore.Hosting
{
    /// <summary>
    /// Web主机扩展类，提供数据库迁移和环境检测功能
    /// </summary>
    public static class IWebHostExtensions
    {
        /// <summary>
        /// 检查应用程序是否运行在Kubernetes环境中
        /// </summary>
        /// <param name="webHost">Web主机实例</param>
        /// <returns>如果在Kubernetes中运行则返回true，否则返回false</returns>
        public static bool IsInKubernetes(this IWebHost webHost)
        {
            var cfg = webHost.Services.GetService<IConfiguration>();
            var orchestratorType = cfg.GetValue<string>("OrchestratorType");
            return orchestratorType?.ToUpper() == "K8S";
        }

        /// <summary>
        /// 执行数据库迁移并应用种子数据
        /// </summary>
        /// <typeparam name="TContext">数据库上下文类型</typeparam>
        /// <param name="webHost">Web主机实例</param>
        /// <param name="seeder">种子数据填充委托</param>
        /// <returns>Web主机实例，用于链式调用</returns>
        public static IWebHost MigrateDbContext<TContext>(this IWebHost webHost, Action<TContext, IServiceProvider> seeder) where TContext : DbContext
        {
            // 检查是否在Kubernetes环境中运行
            var underK8s = webHost.IsInKubernetes();

            // 创建服务作用域以获取所需服务
            using var scope = webHost.Services.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<TContext>>();
            var context = services.GetService<TContext>();

            try
            {
                logger.LogInformation("Migrating database associated with context {DbContextName}", typeof(TContext).Name);

                if (underK8s)
                {
                    // 在Kubernetes环境中，直接执行迁移和种子数据填充
                    // 不需要重试策略，由K8s处理服务重启
                    InvokeSeeder(seeder, context, services);
                }
                else
                {
                    // 非Kubernetes环境下，使用Polly实现重试策略
                    var retries = 10;
                    var retry = Policy.Handle<SqlException>()
                        .WaitAndRetry(
                            retryCount: retries,
                            // 指数退避算法：重试间隔随着重试次数增加而增加
                            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                            // 重试发生时的回调函数，用于记录警告日志
                            onRetry: (exception, timeSpan, retry, ctx) =>
                            {
                                logger.LogWarning(exception, "[{prefix}] Exception {ExceptionType} with message {Message} detected on attempt {retry} of {retries}",
                                    nameof(TContext), exception.GetType().Name, exception.Message, retry, retries);
                            });

                    // 当使用Docker Compose运行时，SQL Server容器可能尚未就绪
                    // 此重试策略处理网络相关异常，而DbContext的重试仅适用于瞬态异常
                    // 注意：在某些编排器中不应用此策略（让编排器重新创建失败的服务）
                    retry.Execute(() => InvokeSeeder(seeder, context, services));
                }

                logger.LogInformation("Migrated database associated with context {DbContextName}", typeof(TContext).Name);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while migrating the database used on context {DbContextName}", typeof(TContext).Name);
                if (underK8s)
                {
                    throw;  // 在Kubernetes环境中重新抛出异常，因为我们依赖K8s重新运行Pod
                }
            }

            return webHost;
        }

        /// <summary>
        /// 执行数据库迁移并应用种子数据填充
        /// </summary>
        /// <typeparam name="TContext">数据库上下文类型</typeparam>
        /// <param name="seeder">种子数据填充委托</param>
        /// <param name="context">数据库上下文实例</param>
        /// <param name="services">服务提供者</param>
        private static void InvokeSeeder<TContext>(Action<TContext, IServiceProvider> seeder, TContext context, IServiceProvider services)
            where TContext : DbContext
        {
            // 执行未应用的迁移
            context.Database.Migrate();
            // 执行种子数据填充
            seeder(context, services);
        }
    }
}
