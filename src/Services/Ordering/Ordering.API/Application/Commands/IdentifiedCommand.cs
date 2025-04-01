namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.Commands;

// 泛型类 IdentifiedCommand 定义，继承自 IRequest<R> 接口
// T 必须是 IRequest<R> 的实现，这样确保传入的命令符合请求（请求-响应）模式
public class IdentifiedCommand<T, R> : IRequest<R>
    where T : IRequest<R>
{
    // Command 属性：存储实际的命令对象，该命令对象符合 IRequest<R>
    public T Command { get; }

    // Id 属性：用于标识命令的唯一标识符，确保每个命令实例可以被唯一辨识
    public Guid Id { get; }

    // 构造函数：接收一个命令对象和一个 Guid 标识符进行初始化
    public IdentifiedCommand(T command, Guid id)
    {
        Command = command;
        Id = id;
    }
}
