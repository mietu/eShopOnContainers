namespace Microsoft.eShopOnContainers.Services.Ordering.Infrastructure.EntityConfigurations;

/// <summary>
/// 订单明细项的实体类型配置类，此类用于配置 OrderItem 实体在数据库中的映射规则。
/// </summary>
class OrderItemEntityTypeConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> orderItemConfiguration)
    {
        // 将 OrderItem 映射到数据库中的 "orderItems" 表，使用默认的 schema
        orderItemConfiguration.ToTable("orderItems", OrderingContext.DEFAULT_SCHEMA);

        // 设置 Id 属性为主键
        orderItemConfiguration.HasKey(o => o.Id);

        // 忽略 DomainEvents 属性，不将其映射到数据库中
        orderItemConfiguration.Ignore(b => b.DomainEvents);

        // 对 Id 属性使用 HiLo 序列生成策略，序列名称为 "orderitemseq"
        orderItemConfiguration.Property(o => o.Id)
            .UseHiLo("orderitemseq");

        // 配置 OrderId 为必填字段
        orderItemConfiguration.Property<int>("OrderId")
            .IsRequired();

        // 配置私有字段 _discount：映射到 "Discount" 列，使用字段直接访问，并标记为必填
        orderItemConfiguration
            .Property<decimal>("_discount")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("Discount")
            .IsRequired();

        // 配置 ProductId 为必填字段
        orderItemConfiguration.Property<int>("ProductId")
            .IsRequired();

        // 配置私有字段 _productName：映射到 "ProductName" 列，使用字段直接访问，并标记为必填
        orderItemConfiguration
            .Property<string>("_productName")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("ProductName")
            .IsRequired();

        // 配置私有字段 _unitPrice：映射到 "UnitPrice" 列，使用字段直接访问，并标记为必填
        orderItemConfiguration
            .Property<decimal>("_unitPrice")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("UnitPrice")
            .IsRequired();

        // 配置私有字段 _units：映射到 "Units" 列，使用字段直接访问，并标记为必填
        orderItemConfiguration
            .Property<int>("_units")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("Units")
            .IsRequired();

        // 配置私有字段 _pictureUrl：映射到 "PictureUrl" 列，使用字段直接访问，此字段可空
        orderItemConfiguration
            .Property<string>("_pictureUrl")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("PictureUrl")
            .IsRequired(false);
    }
}
