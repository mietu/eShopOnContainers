namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.Commands;

// 此命令用于设置订单状态为“库存确认”
// 它实现了IRequest<bool>接口，表示执行该命令后返回一个布尔值，以确认操作是否成功
public class SetStockConfirmedOrderStatusCommand : IRequest<bool>
{
    // DataMember属性指示该属性可用于序列化和反序列化操作
    [DataMember]
    public int OrderNumber { get; private set; } // 订单编号，外部只能读取，内部通过构造函数设置

    // 构造函数接收订单编号，用于初始化命令实例
    public SetStockConfirmedOrderStatusCommand(int orderNumber)
    {
        OrderNumber = orderNumber;
    }
}
