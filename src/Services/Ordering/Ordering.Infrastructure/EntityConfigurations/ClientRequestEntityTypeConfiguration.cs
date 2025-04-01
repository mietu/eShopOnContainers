namespace Microsoft.eShopOnContainers.Services.Ordering.Infrastructure.EntityConfigurations;

// 该类实现了 IEntityTypeConfiguration 接口，用于配置 ClientRequest 实体在数据库中的映射关系
class ClientRequestEntityTypeConfiguration : IEntityTypeConfiguration<ClientRequest>
{
    // 在此方法中配置实体到数据库表的映射关系及属性设置
    public void Configure(EntityTypeBuilder<ClientRequest> requestConfiguration)
    {
        // 将 ClientRequest 实体映射到数据库中的 "requests" 表，
        // 使用 OrderingContext.DEFAULT_SCHEMA 指定默认的模式(schema)
        requestConfiguration.ToTable("requests", OrderingContext.DEFAULT_SCHEMA);

        // 配置 Id 属性为表的主键
        requestConfiguration.HasKey(cr => cr.Id);

        // 配置 Name 属性为必填项，即不能为空（IsRequired）
        requestConfiguration.Property(cr => cr.Name).IsRequired();

        // 配置 Time 属性为必填项，即不能为空（IsRequired）
        requestConfiguration.Property(cr => cr.Time).IsRequired();
    }
}
