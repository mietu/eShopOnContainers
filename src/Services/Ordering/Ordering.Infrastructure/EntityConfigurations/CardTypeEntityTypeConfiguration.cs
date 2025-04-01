namespace Microsoft.eShopOnContainers.Services.Ordering.Infrastructure.EntityConfigurations;

/// <summary>
/// 用于配置 CardType 实体与数据库表的映射关系
/// </summary>
class CardTypeEntityTypeConfiguration : IEntityTypeConfiguration<CardType>
{
    public void Configure(EntityTypeBuilder<CardType> cardTypesConfiguration)
    {
        // 指定映射的表名为 "cardtypes"，并使用 OrderingContext 中定义的默认架构
        cardTypesConfiguration.ToTable("cardtypes", OrderingContext.DEFAULT_SCHEMA);

        // 配置实体主键为 Id 属性
        cardTypesConfiguration.HasKey(ct => ct.Id);

        // 配置 Id 属性：
        // 1. 默认值为 1
        // 2. 不自动生成值（即使用 ValueGeneratedNever）
        // 3. 为必填字段
        cardTypesConfiguration.Property(ct => ct.Id)
            .HasDefaultValue(1)
            .ValueGeneratedNever()
            .IsRequired();

        // 配置 Name 属性：
        // 1. 最大长度为 200 个字符
        // 2. 为必填字段
        cardTypesConfiguration.Property(ct => ct.Name)
            .HasMaxLength(200)
            .IsRequired();
    }
}
