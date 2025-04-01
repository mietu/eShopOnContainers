namespace Basket.API.Infrastructure.Exceptions;

// 定义 BasketDomainException 类，继承自基类 Exception
public class BasketDomainException : Exception
{
    // 默认构造函数，不传入任何错误信息
    public BasketDomainException()
    { }

    // 构造函数，接收一个错误信息字符串
    public BasketDomainException(string message)
        : base(message) // 调用基类构造函数，传递错误信息
    { }

    // 构造函数，接收一个错误信息字符串和一个内部异常对象
    // 内部异常用于提供更详细的错误来源
    public BasketDomainException(string message, Exception innerException)
        : base(message, innerException) // 调用基类构造函数，传递错误信息和内部异常
    { }
}

