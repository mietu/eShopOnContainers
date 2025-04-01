namespace Microsoft.eShopOnContainers.Services.Catalog.API.Infrastructure.EntityConfigurations;

// 定义 CatalogType 实体的映射配置，继承自 IEntityTypeConfiguration<CatalogType>
class CatalogTypeEntityTypeConfiguration : IEntityTypeConfiguration<CatalogType>
{
    // 配置 CatalogType 实体的行为
    public void Configure(EntityTypeBuilder<CatalogType> builder)
    {
        // 指定实体映射到的数据库表名为 "CatalogType"
        builder.ToTable("CatalogType");

        // 配置实体的主键为 Id 属性
        builder.HasKey(ci => ci.Id);

        // 对 Id 属性进行配置：使用 HiLo 序列生成器，指定序列名称为 "catalog_type_hilo"，并且是必填项
        builder.Property(ci => ci.Id)
            .UseHiLo("catalog_type_hilo")
            .IsRequired();

        // 对 Type 属性进行配置：为必填字段，最大长度限制为 100 个字符
        builder.Property(cb => cb.Type)
            .IsRequired()
            .HasMaxLength(100);
    }
}
