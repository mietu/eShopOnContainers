namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.Commands;

// 该类表示发货订单的命令，继承自 IRequest<bool> 表示这是一个请求，并期望返回一个 bool 类型的结果
public class ShipOrderCommand : IRequest<bool>
{
    // 使用 DataMember 特性标记该属性用于序列化，这个属性表示订单号
    [DataMember]
    public int OrderNumber { get; private set; }

    // 构造函数，通过传入订单号初始化命令
    public ShipOrderCommand(int orderNumber)
    {
        OrderNumber = orderNumber;
    }
}
