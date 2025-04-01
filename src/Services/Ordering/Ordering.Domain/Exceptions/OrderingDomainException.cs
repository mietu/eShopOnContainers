namespace Microsoft.eShopOnContainers.Services.Ordering.Domain.Exceptions;

/// <summary>
/// 域层异常类型，用于表示业务领域中的异常情况
/// </summary>
public class OrderingDomainException : Exception
{
    // 默认构造函数
    // 用于在没有异常消息的情况下创建异常实例
    public OrderingDomainException()
    { }

    // 构造函数，带有异常消息
    // message: 描述异常的消息
    public OrderingDomainException(string message)
        : base(message)
    { }

    // 构造函数，带有异常消息和内部异常
    // message: 描述异常的消息
    // innerException: 导致当前异常的内部异常
    public OrderingDomainException(string message, Exception innerException)
        : base(message, innerException)
    { }
}
