namespace Microsoft.eShopOnContainers.Services.Catalog.API.Infrastructure.EntityConfigurations;

// 实现 IEntityTypeConfiguration 接口，用于配置 CatalogBrand 实体在数据库中的映射
class CatalogBrandEntityTypeConfiguration : IEntityTypeConfiguration<CatalogBrand>
{
    public void Configure(EntityTypeBuilder<CatalogBrand> builder)
    {
        // 配置实体对应的数据库表名为 "CatalogBrand"
        builder.ToTable("CatalogBrand");

        // 指定实体的主键为 Id 属性
        builder.HasKey(ci => ci.Id);

        // 配置 Id 属性:
        // 使用 HiLo 算法生成主键值，数据库序列名称为 "catalog_brand_hilo"
        // 并设置该属性为必填项
        builder.Property(ci => ci.Id)
            .UseHiLo("catalog_brand_hilo")
            .IsRequired();

        // 配置 Brand 属性:
        // 设置为必填项，并限制最大长度为 100 个字符
        builder.Property(cb => cb.Brand)
            .IsRequired()
            .HasMaxLength(100);
    }
}
