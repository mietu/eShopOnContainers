namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.Commands;

// 该命令用于设置订单状态为“Awaiting Validation”（待验证）
// 实现 IRequest<bool> 接口，表示此命令发送后会返回一个 bool 类型的结果
public class SetAwaitingValidationOrderStatusCommand : IRequest<bool>
{
    // DataMember 特性用于序列化，确保 OrderNumber 字段在传输中被正确处理
    // OrderNumber 表示订单编号，是该命令的核心数据
    [DataMember]
    public int OrderNumber { get; private set; }

    // 构造函数，初始化命令时需要传入订单编号
    public SetAwaitingValidationOrderStatusCommand(int orderNumber)
    {
        OrderNumber = orderNumber;
    }
}
