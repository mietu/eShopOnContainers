namespace Microsoft.eShopOnContainers.Services.Ordering.API.Infrastructure.Factories
{
    // 设计时 DbContext 工厂类，用于在设计时（例如执行 EF Core 命令迁移）创建 OrderingContext 实例
    public class OrderingDbContextFactory : IDesignTimeDbContextFactory<OrderingContext>
    {
        // 根据 args 参数创建并返回 OrderingContext 实例
        public OrderingContext CreateDbContext(string[] args)
        {
            // 使用 ConfigurationBuilder 构建应用配置
            var config = new ConfigurationBuilder()
                // 设置当前路径为基路径，用于搜索配置文件
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory()))
                // 添加 JSON 格式的配置文件 appsettings.json
                .AddJsonFile("appsettings.json")
                // 添加环境变量，允许通过环境变量覆盖配置文件中的设置
                .AddEnvironmentVariables()
                // 构建 IConfigurationRoot 对象
                .Build();

            // 创建 OrderingContext 的 DbContextOptions 构建器
            var optionsBuilder = new DbContextOptionsBuilder<OrderingContext>();

            // 配置使用 SQL Server 数据库
            // 连接字符串从配置中读取，并指定 Migrations 程序集为 "Ordering.API"
            optionsBuilder.UseSqlServer(
                config["ConnectionString"],
                sqlServerOptionsAction: o => o.MigrationsAssembly("Ordering.API"));

            // 根据配置选项创建并返回 OrderingContext 实例
            return new OrderingContext(optionsBuilder.Options);
        }
    }
}