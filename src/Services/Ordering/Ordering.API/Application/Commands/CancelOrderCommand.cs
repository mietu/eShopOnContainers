namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.Commands;

// CancelOrderCommand 类用于取消订单的操作，目前它实现了 IRequest<bool> 接口，
// 可以在命令处理器中处理命令并返回一个表示操作结果的布尔值。
public class CancelOrderCommand : IRequest<bool>
{
    // DataMember 特性表示该属性可以被序列化，
    // 这对于分布式系统中数据的传输和持久化很有用。
    [DataMember]
    public int OrderNumber { get; set; }

    // 默认构造函数，允许在不指定订单编号的情况下实例化对象。
    public CancelOrderCommand()
    {
    }

    // 带参构造函数，用于在创建对象时指定订单编号，
    // 这样可以确保对象一经创建就包含必要的数据。
    public CancelOrderCommand(int orderNumber)
    {
        OrderNumber = orderNumber;
    }
}
