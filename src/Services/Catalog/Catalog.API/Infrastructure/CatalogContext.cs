namespace Microsoft.eShopOnContainers.Services.Catalog.API.Infrastructure;

// CatalogContext 类继承自 Entity Framework Core 的 DbContext，
// 用于与数据库交互以及管理 CatalogItem、CatalogBrand 和 CatalogType 实体。
public class CatalogContext : DbContext
{
    // 构造函数通过依赖注入传入 DbContextOptions 对象
    public CatalogContext(DbContextOptions<CatalogContext> options) : base(options)
    {
    }

    // 定义 CatalogItems 实体集，映射数据库中的 CatalogItems 表
    public DbSet<CatalogItem> CatalogItems { get; set; }
    // 定义 CatalogBrands 实体集，映射数据库中的 CatalogBrands 表
    public DbSet<CatalogBrand> CatalogBrands { get; set; }
    // 定义 CatalogTypes 实体集，映射数据库中的 CatalogTypes 表
    public DbSet<CatalogType> CatalogTypes { get; set; }

    // 重写 OnModelCreating 方法，在创建模型时配置实体与数据库之间的映射关系
    protected override void OnModelCreating(ModelBuilder builder)
    {
        // 应用 CatalogBrand 实体的配置
        builder.ApplyConfiguration(new CatalogBrandEntityTypeConfiguration());
        // 应用 CatalogType 实体的配置
        builder.ApplyConfiguration(new CatalogTypeEntityTypeConfiguration());
        // 应用 CatalogItem 实体的配置
        builder.ApplyConfiguration(new CatalogItemEntityTypeConfiguration());
    }
}


// CatalogContextDesignFactory 用于设计时工厂，用于在设计时创建 CatalogContext 实例（例如用于迁移）
public class CatalogContextDesignFactory : IDesignTimeDbContextFactory<CatalogContext>
{
    public CatalogContext CreateDbContext(string[] args)
    {
        // 创建 DbContextOptionsBuilder，并指定 SQL Server 数据库连接字符串
        var optionsBuilder = new DbContextOptionsBuilder<CatalogContext>()
            .UseSqlServer("Server=.;Initial Catalog=Microsoft.eShopOnContainers.Services.CatalogDb;Integrated Security=true");

        // 返回使用指定选项构造的 CatalogContext 实例
        return new CatalogContext(optionsBuilder.Options);
    }
}
