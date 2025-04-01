namespace Microsoft.eShopOnContainers.Services.Ordering.Infrastructure.EntityConfigurations;

// 实现 IEntityTypeConfiguration 接口，以便在 DbContext 中应用该配置
class OrderStatusEntityTypeConfiguration : IEntityTypeConfiguration<OrderStatus>
{
    // Configure 方法用于定义实体的表映射及属性约束
    public void Configure(EntityTypeBuilder<OrderStatus> orderStatusConfiguration)
    {
        // 指定映射到数据库中表名为 "orderstatus"，并使用默认模式 (OrderingContext.DEFAULT_SCHEMA)
        orderStatusConfiguration.ToTable("orderstatus", OrderingContext.DEFAULT_SCHEMA);

        // 配置实体的主键为 Id 属性
        orderStatusConfiguration.HasKey(o => o.Id);

        // 配置 Id 属性：
        // - 默认值设为 1
        // - 不自动由数据库生成 (ValueGeneratedNever)
        // - 设置为必填字段 (IsRequired)
        orderStatusConfiguration.Property(o => o.Id)
            .HasDefaultValue(1)
            .ValueGeneratedNever()
            .IsRequired();

        // 配置 Name 属性：
        // - 最大长度限制为 200
        // - 设置为必填字段 (IsRequired)
        orderStatusConfiguration.Property(o => o.Name)
            .HasMaxLength(200)
            .IsRequired();
    }
}
