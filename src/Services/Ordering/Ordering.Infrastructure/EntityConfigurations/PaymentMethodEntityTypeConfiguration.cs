namespace Microsoft.eShopOnContainers.Services.Ordering.Infrastructure.EntityConfigurations;

// PaymentMethod 实体配置，用于指定与数据库表及字段的映射关系
class PaymentMethodEntityTypeConfiguration : IEntityTypeConfiguration<PaymentMethod>
{
    public void Configure(EntityTypeBuilder<PaymentMethod> paymentConfiguration)
    {
        // 指定 PaymentMethod 映射到的数据库表名称以及 Schema
        paymentConfiguration.ToTable("paymentmethods", OrderingContext.DEFAULT_SCHEMA);

        // 配置主键为 Id 属性
        paymentConfiguration.HasKey(b => b.Id);

        // 忽略领域事件，不映射到数据库中
        paymentConfiguration.Ignore(b => b.DomainEvents);

        // 配置 Id 属性使用 HiLo 序列生成器，并指定序列名及 Schema
        paymentConfiguration.Property(b => b.Id)
            .UseHiLo("paymentseq", OrderingContext.DEFAULT_SCHEMA);

        // 配置 BuyerId 为必填字段（影子属性，非实体显式定义）
        paymentConfiguration.Property<int>("BuyerId")
            .IsRequired();

        // 配置持卡人名称对应的字段 _cardHolderName，使用字段访问方式
        paymentConfiguration
            .Property<string>("_cardHolderName")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("CardHolderName")
            .HasMaxLength(200)  // 最大长度 200
            .IsRequired();

        // 配置支付卡别名对应的字段 _alias，使用字段访问方式
        paymentConfiguration
            .Property<string>("_alias")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("Alias")
            .HasMaxLength(200)  // 最大长度 200
            .IsRequired();

        // 配置支付卡号对应的字段 _cardNumber，使用字段访问方式
        paymentConfiguration
            .Property<string>("_cardNumber")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("CardNumber")
            .HasMaxLength(25)  // 最大长度 25
            .IsRequired();

        // 配置支付卡过期日期对应的字段 _expiration，使用字段访问方式
        paymentConfiguration
            .Property<DateTime>("_expiration")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("Expiration")
            .HasMaxLength(25)  // 此行虽然对 DateTime 设置长度，但实际意义可能不大
            .IsRequired();

        // 配置卡类型标识对应的字段 _cardTypeId，使用字段访问方式
        paymentConfiguration
            .Property<int>("_cardTypeId")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("CardTypeId")
            .IsRequired();

        // 配置 PaymentMethod 与 CardType 之间的关联关系
        // 一个 PaymentMethod 有一个 CardType，通过 _cardTypeId 进行连接
        paymentConfiguration.HasOne(p => p.CardType)
            .WithMany()
            .HasForeignKey("_cardTypeId");
    }
}
