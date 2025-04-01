namespace Microsoft.eShopOnContainers.Services.Basket.API.IntegrationEvents.Events;

// 此记录继承自 IntegrationEvent，表示产品价格变化的集成事件
// 使用 record 关键字可自动生成不可变的数据类型以及结构化优先的相等比较
public record ProductPriceChangedIntegrationEvent : IntegrationEvent
{
    // 产品唯一标识，使用私有 init 修饰符，保证只能在构造函数中或初始化时设置
    public int ProductId { get; private init; }

    // 更新后的价格
    public decimal NewPrice { get; private init; }

    // 原始价格
    public decimal OldPrice { get; private init; }

    /// <summary>
    /// 构造函数：初始化产品价格变化事件，记录产品ID、新价格及原价格
    /// </summary>
    /// <param name="productId">产品唯一标识</param>
    /// <param name="newPrice">更新后的价格</param>
    /// <param name="oldPrice">修改前的原始价格</param>
    public ProductPriceChangedIntegrationEvent(int productId, decimal newPrice, decimal oldPrice)
    {
        ProductId = productId;
        NewPrice = newPrice;
        OldPrice = oldPrice;
    }
}
