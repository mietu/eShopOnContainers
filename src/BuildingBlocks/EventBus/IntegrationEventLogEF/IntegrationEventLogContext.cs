namespace Microsoft.eShopOnContainers.BuildingBlocks.IntegrationEventLogEF;

// IntegrationEventLogContext 用于管理集成事件日志的数据库上下文
public class IntegrationEventLogContext : DbContext
{
    // 构造函数，传入 DbContextOptions 用于配置数据库连接等参数
    public IntegrationEventLogContext(DbContextOptions<IntegrationEventLogContext> options) : base(options)
    {
    }

    // DbSet 表示数据库中 IntegrationEventLogEntry 实体的集合，对应数据库表中的记录
    public DbSet<IntegrationEventLogEntry> IntegrationEventLogs { get; set; }

    // 重写 OnModelCreating 方法，用于配置实体的映射关系
    protected override void OnModelCreating(ModelBuilder builder)
    {
        // 使用 ConfigureIntegrationEventLogEntry 方法配置 IntegrationEventLogEntry 实体
        builder.Entity<IntegrationEventLogEntry>(ConfigureIntegrationEventLogEntry);
    }

    // 配置 IntegrationEventLogEntry 实体的映射规则
    void ConfigureIntegrationEventLogEntry(EntityTypeBuilder<IntegrationEventLogEntry> builder)
    {
        // 指定实体映射到数据库中的 "IntegrationEventLog" 表
        builder.ToTable("IntegrationEventLog");

        // 定义主键为 EventId 属性
        builder.HasKey(e => e.EventId);

        // 配置各属性为必填项
        builder.Property(e => e.EventId)
            .IsRequired();

        builder.Property(e => e.Content)
            .IsRequired();

        builder.Property(e => e.CreationTime)
            .IsRequired();

        builder.Property(e => e.State)
            .IsRequired();

        builder.Property(e => e.TimesSent)
            .IsRequired();

        builder.Property(e => e.EventTypeName)
            .IsRequired();
    }
}
