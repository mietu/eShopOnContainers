namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.Commands;

// 该命令用于设置订单状态为已付款，继承 IRequest<bool> 接口用于 MediatR 消息传递
public class SetPaidOrderStatusCommand : IRequest<bool>
{
    // DataMember 特性指示此属性在序列化时将被保留（保证数据传输正确）
    [DataMember]
    public int OrderNumber { get; private set; }  // 订单编号，标识唯一订单

    // 构造函数，初始化 SetPaidOrderStatusCommand 实例，同时接收订单号作为参数
    public SetPaidOrderStatusCommand(int orderNumber)
    {
        OrderNumber = orderNumber;
    }
}
