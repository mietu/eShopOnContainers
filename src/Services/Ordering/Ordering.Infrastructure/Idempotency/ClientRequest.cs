namespace Microsoft.eShopOnContainers.Services.Ordering.Infrastructure.Idempotency;

/// <summary>
/// 表示一个客户端请求，用于实现请求幂等性控制。
/// </summary>
public class ClientRequest
{
    // 客户请求的唯一标识符
    public Guid Id { get; set; }

    // 客户请求的名称
    public string Name { get; set; }

    // 请求的时间戳，表示请求发送的时间
    public DateTime Time { get; set; }
}
