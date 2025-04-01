using Microsoft.eShopOnContainers.Services.Ordering.Domain.SeedWork;

namespace Microsoft.eShopOnContainers.Services.Ordering.Domain.AggregatesModel.BuyerAggregate;

/// <remarks>
/// 本类用于定义银行卡类型。
/// 应该将CardType类标记为抽象类，并使用受保护的构造函数以封装已知的枚举类型，
/// 但由于OrderingContextSeed需要使用该构造函数从CSV文件加载卡类型，目前无法这样实现。
/// </remarks>
public class CardType : Enumeration
{
    // 定义Amex卡类型，ID为1，名称为"Amex"
    public static CardType Amex = new(1, nameof(Amex));

    // 定义Visa卡类型，ID为2，名称为"Visa"
    public static CardType Visa = new(2, nameof(Visa));

    // 定义MasterCard卡类型，ID为3，名称为"MasterCard"
    public static CardType MasterCard = new(3, nameof(MasterCard));

    // 构造函数，接受卡类型的ID和名称，并调用基类的构造函数初始化
    public CardType(int id, string name)
        : base(id, name)
    {
    }
}
