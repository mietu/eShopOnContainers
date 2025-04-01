namespace Microsoft.eShopOnContainers.Web.Shopping.HttpAggregator.Infrastructure;

// GrpcExceptionInterceptor 类用于拦截 gRPC 调用中发生的异常，
// 并通过日志记录错误信息。
public class GrpcExceptionInterceptor : Interceptor
{
    // 日志记录器，用于输出错误信息
    private readonly ILogger<GrpcExceptionInterceptor> _logger;

    // 构造函数注入 ILogger 依赖
    public GrpcExceptionInterceptor(ILogger<GrpcExceptionInterceptor> logger)
    {
        _logger = logger;
    }

    // 重写 AsyncUnaryCall 方法，以拦截 gRPC 的异步调用
    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        // 调用实际的 gRPC 请求，并获取 AsyncUnaryCall 对象
        var call = continuation(request, context);

        // 返回新的 AsyncUnaryCall 对象，包装了对响应处理的逻辑
        return new AsyncUnaryCall<TResponse>(
            HandleResponse(call.ResponseAsync), // 包装响应任务以处理异常
            call.ResponseHeadersAsync,          // 响应头任务保持原样
            call.GetStatus,                     // 获取状态的方法
            call.GetTrailers,                   // 获取尾随消息的方法
            call.Dispose                        // 处理清理资源的方法
        );
    }

    // 处理响应任务，捕获 RpcException 异常并记录错误日志
    private async Task<TResponse> HandleResponse<TResponse>(Task<TResponse> task)
    {
        try
        {
            // 等待并返回响应结果
            var response = await task;
            return response;
        }
        catch (RpcException e)
        {
            // 记录异常错误（状态码和消息）
            _logger.LogError("Error calling via grpc: {Status} - {Message}", e.Status, e.Message);
            // 当发生异常时返回默认值（例如null）
            return default;
        }
    }
}
