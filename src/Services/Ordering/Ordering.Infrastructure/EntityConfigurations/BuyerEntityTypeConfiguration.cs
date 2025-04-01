namespace Microsoft.eShopOnContainers.Services.Ordering.Infrastructure.EntityConfigurations;

// BuyerEntityTypeConfiguration 类用于配置 Buyer 实体在 EF Core 中如何映射到数据库。
class BuyerEntityTypeConfiguration : IEntityTypeConfiguration<Buyer>
{
    // Configure 方法中定义了 Buyer 实体的数据库映射规则
    public void Configure(EntityTypeBuilder<Buyer> buyerConfiguration)
    {
        // 设定 Buyer 实体映射到的数据库表名称为 "buyers"，并使用 OrderingContext.DEFAULT_SCHEMA 指定的架构
        buyerConfiguration.ToTable("buyers", OrderingContext.DEFAULT_SCHEMA);

        // 设定 Buyer 实体的主键为 Id 属性
        buyerConfiguration.HasKey(b => b.Id);

        // 忽略 DomainEvents 属性，不将其映射到数据库，这通常用于领域事件
        buyerConfiguration.Ignore(b => b.DomainEvents);

        // 配置 Id 属性使用 HiLo 序列生成器，序列名称为 "buyerseq"，并指定使用默认架构
        buyerConfiguration.Property(b => b.Id)
            .UseHiLo("buyerseq", OrderingContext.DEFAULT_SCHEMA);

        // 配置 IdentityGuid 属性，设定最大长度为200且不允许为空
        buyerConfiguration.Property(b => b.IdentityGuid)
            .HasMaxLength(200)
            .IsRequired();

        // 为 IdentityGuid 属性创建唯一索引，确保每个买家身份对应一个唯一的记录
        buyerConfiguration.HasIndex("IdentityGuid")
            .IsUnique(true);

        // 配置 Name 属性，不做额外的约束或转换
        buyerConfiguration.Property(b => b.Name);

        // 配置 Buyer 与 PaymentMethods 的一对多关系：
        // 一个 Buyer 可以拥有多个 PaymentMethod，设置外键为 "BuyerId"，删除 Buyer 时级联删除关联的 PaymentMethod
        buyerConfiguration.HasMany(b => b.PaymentMethods)
            .WithOne()
            .HasForeignKey("BuyerId")
            .OnDelete(DeleteBehavior.Cascade);

        // 配置 PaymentMethods 导航属性的数据访问模式为 Field（字段直接访问）以提高性能或隐藏内部实现
        var navigation = buyerConfiguration.Metadata.FindNavigation(nameof(Buyer.PaymentMethods));
        navigation.SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
