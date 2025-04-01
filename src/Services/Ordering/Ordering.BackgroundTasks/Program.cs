using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Ordering.BackgroundTasks.Extensions;
using Serilog;
using System.IO;

namespace Ordering.BackgroundTasks
{
    public class Program
    {
        // 通过反射获取程序集名称，用于标识应用程序名称
        public static readonly string AppName = typeof(Program).Assembly.GetName().Name;

        // 程序入口点
        public static void Main(string[] args)
        {
            // 创建并运行主机
            CreateHostBuilder(args).Run();
        }

        // 创建并配置应用程序主机
        public static IHost CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                // 使用Autofac作为依赖注入的容器
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                // 配置Web主机默认设置，并设置Startup类
                .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
                // 配置应用程序配置（例如json文件、环境变量、命令行参数）
                .ConfigureAppConfiguration((host, builder) =>
                {
                    // 设置基路径为当前目录
                    builder.SetBasePath(Directory.GetCurrentDirectory());
                    // 添加appsettings.json 配置文件，设为可选
                    builder.AddJsonFile("appsettings.json", optional: true);
                    // 根据当前环境添加特定的appsettings文件
                    builder.AddJsonFile($"appsettings.{host.HostingEnvironment.EnvironmentName}.json", optional: true);
                    // 添加环境变量配置
                    builder.AddEnvironmentVariables();
                    // 添加命令行参数配置
                    builder.AddCommandLine(args);
                })
                // 配置日志记录，使用Serilog日志框架
                .ConfigureLogging((host, builder) => builder.UseSerilog(host.Configuration).AddSerilog())
                // 构建并返回主机实例
                .Build();
    }
}
