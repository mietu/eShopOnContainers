namespace Microsoft.eShopOnContainers.Services.Catalog.API.Infrastructure.EntityConfigurations;

/// <summary>
/// 定义 CatalogItem 实体的数据库映射配置
/// </summary>
class CatalogItemEntityTypeConfiguration : IEntityTypeConfiguration<CatalogItem>
{
    public void Configure(EntityTypeBuilder<CatalogItem> builder)
    {
        // 映射到名为 "Catalog" 的表
        builder.ToTable("Catalog");

        // 配置 Id 属性：
        // 使用 HiLo 算法生成 ID，并确保该字段为必填项
        builder.Property(ci => ci.Id)
            .UseHiLo("catalog_hilo")
            .IsRequired();

        // 配置 Name 属性：
        // 设置为必填，并限制最大长度为 50 位字符
        builder.Property(ci => ci.Name)
            .IsRequired(true)
            .HasMaxLength(50);

        // 配置 Price 属性：
        // 设置为必填项
        builder.Property(ci => ci.Price)
            .IsRequired(true);

        // 配置 PictureFileName 属性：
        // 设置为可选项
        builder.Property(ci => ci.PictureFileName)
            .IsRequired(false);

        // 忽略 PictureUri 属性，不进行数据库映射
        builder.Ignore(ci => ci.PictureUri);

        // 配置与 CatalogBrand 实体的关系：
        // CatalogItem 拥有一个 CatalogBrand，多对一关系，外键为 CatalogBrandId
        builder.HasOne(ci => ci.CatalogBrand)
            .WithMany()
            .HasForeignKey(ci => ci.CatalogBrandId);

        // 配置与 CatalogType 实体的关系：
        // CatalogItem 拥有一个 CatalogType，多对一关系，外键为 CatalogTypeId
        builder.HasOne(ci => ci.CatalogType)
            .WithMany()
            .HasForeignKey(ci => ci.CatalogTypeId);
    }
}
