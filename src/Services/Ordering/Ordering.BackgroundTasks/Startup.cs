namespace Ordering.BackgroundTasks
{
    using HealthChecks.UI.Client;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Diagnostics.HealthChecks;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Ordering.BackgroundTasks.Extensions;
    using Ordering.BackgroundTasks.Services;

    // Startup类：配置服务和HTTP请求管道
    public class Startup
    {
        // 构造函数接受IConfiguration传入配置信息
        public Startup(IConfiguration configuration)
        {
            // 将传入的配置赋值给Configuration属性
            Configuration = configuration;
        }

        // 配置文件属性
        public IConfiguration Configuration { get; }

        // 注册服务的方法，可供派生类重写扩展
        public virtual void ConfigureServices(IServiceCollection services)
        {
            // 以下链式调用注册了自定义的健康检查、后台任务设置、选项、托管服务和事件总线
            services.AddCustomHealthCheck(this.Configuration) // 添加自定义健康检查服务
                .Configure<BackgroundTaskSettings>(this.Configuration) // 绑定背景任务设置到配置
                .AddOptions() // 添加选项支持
                .AddHostedService<GracePeriodManagerService>() // 注册托管服务：GracePeriodManagerService
                .AddEventBus(this.Configuration); // 添加事件总线服务
        }

        // 配置HTTP请求管道
        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            app.UseRouting(); // 启用路由中间件

            // 配置终结点
            app.UseEndpoints(endpoints =>
            {
                // 设置健康检查终结点"/hc"，并通过UIResponseWriter构建健康检查UI响应
                endpoints.MapHealthChecks("/hc", new HealthCheckOptions()
                {
                    Predicate = _ => true, // 检查所有的健康检查项
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse // 使用UI格式响应
                });

                // 设置存活探测终结点"/liveness"，仅检查名称包含"self"的健康检查项
                endpoints.MapHealthChecks("/liveness", new HealthCheckOptions
                {
                    Predicate = r => r.Name.Contains("self")
                });
            });
        }
    }
}
