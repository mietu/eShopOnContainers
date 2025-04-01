namespace WebMVC.Services.ModelDTOs;

/// <summary>
/// 表示订单处理动作的不可变记录类
/// </summary>
public record OrderProcessAction
{
    /// <summary>
    /// 获取动作的代码标识
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// 获取动作的显示名称
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 预定义的发货处理动作
    /// </summary>
    public static OrderProcessAction Ship = new OrderProcessAction(nameof(Ship).ToLowerInvariant(), "Ship");

    /// <summary>
    /// 受保护的无参构造函数，用于序列化或子类继承
    /// </summary>
    protected OrderProcessAction()
    {
    }

    /// <summary>
    /// 初始化订单处理动作实例
    /// </summary>
    /// <param name="code">动作代码标识</param>
    /// <param name="name">动作显示名称</param>
    public OrderProcessAction(string code, string name)
    {
        Code = code;
        Name = name;
    }
}
