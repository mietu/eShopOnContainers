namespace Microsoft.eShopOnContainers.Services.Ordering.Domain.Seedwork;

/// <summary>
/// 聚合根标记接口。
/// 
/// 在领域驱动设计中，聚合是一组相关对象的集合，这些对象被视为一个单元，它们的数据一致性需要通过一个聚合根来进行管理。
/// 聚合根是聚合中的主实体，它负责保证整个聚合内部的一致性和业务规则。
/// 实现此接口的类一般代表领域中的聚合根，将通过它来管理聚合中的其他对象。
/// 
/// 该接口不定义任何方法或属性，仅用于标识领域对象是否为聚合根。
/// </summary>
public interface IAggregateRoot
{
    // 此接口为标记接口，无需定义任何成员。
}


