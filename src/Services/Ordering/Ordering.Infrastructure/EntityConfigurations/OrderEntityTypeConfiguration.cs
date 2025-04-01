namespace Microsoft.eShopOnContainers.Services.Ordering.Infrastructure.EntityConfigurations;

/// <summary>
/// 配置 Order 实体的数据库映射关系
/// </summary>
class OrderEntityTypeConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> orderConfiguration)
    {
        // 指定数据表名为 "orders"，并使用默认架构
        orderConfiguration.ToTable("orders", OrderingContext.DEFAULT_SCHEMA);

        // 设置主键为 Order 的 Id 属性
        orderConfiguration.HasKey(o => o.Id);

        // 忽略不需要映射到数据库的 DomainEvents 属性
        orderConfiguration.Ignore(b => b.DomainEvents);

        // 配置 Id 属性使用 HiLo 策略生成主键值
        orderConfiguration.Property(o => o.Id)
            .UseHiLo("orderseq", OrderingContext.DEFAULT_SCHEMA);

        // 配置 Address 值对象，将其持久化为 owned entity 类型
        // EF Core 5 中存在的问题，通过配置 shadow key "OrderId" 解决
        orderConfiguration
            .OwnsOne(o => o.Address, a =>
            {
                // 配置 shadow key 属性 "OrderId" 并使用 HiLo 生成策略
                a.Property<int>("OrderId")
                    .UseHiLo("orderseq", OrderingContext.DEFAULT_SCHEMA);
                // 建立拥有关系
                a.WithOwner();
            });

        // 配置 _buyerId 私有字段映射到 "BuyerId" 列，允许为空
        orderConfiguration
            .Property<int?>("_buyerId")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("BuyerId")
            .IsRequired(false);

        // 配置 _orderDate 私有字段映射到 "OrderDate" 列，并设为必填
        orderConfiguration
            .Property<DateTime>("_orderDate")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("OrderDate")
            .IsRequired();

        // 配置 _orderStatusId 私有字段映射到 "OrderStatusId" 列，并设为必填
        orderConfiguration
            .Property<int>("_orderStatusId")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("OrderStatusId")
            .IsRequired();

        // 配置 _paymentMethodId 私有字段映射到 "PaymentMethodId" 列，允许为空
        orderConfiguration
            .Property<int?>("_paymentMethodId")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("PaymentMethodId")
            .IsRequired(false);

        // 配置 Description 列，可以为空
        orderConfiguration.Property<string>("Description").IsRequired(false);

        // 获取 OrderItems 导航属性的元数据
        var navigation = orderConfiguration.Metadata.FindNavigation(nameof(Order.OrderItems));
        // 按字段方式访问 OrderItems 集合（符合 DDD 封装原则）
        navigation.SetPropertyAccessMode(PropertyAccessMode.Field);

        // 配置与 PaymentMethod 的关系，使用 _paymentMethodId 作为外键，允许为空，
        // 删除 PaymentMethod 时限制级联删除
        orderConfiguration.HasOne<PaymentMethod>()
            .WithMany()
            .HasForeignKey("_paymentMethodId")
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // 配置与 Buyer 的关系，使用 _buyerId 作为外键，允许为空
        orderConfiguration.HasOne<Buyer>()
            .WithMany()
            .IsRequired(false)
            .HasForeignKey("_buyerId");

        // 配置与 OrderStatus 的关系，使用 _orderStatusId 作为外键
        orderConfiguration.HasOne(o => o.OrderStatus)
            .WithMany()
            .HasForeignKey("_orderStatusId");
    }
}
