namespace Microsoft.eShopOnContainers.Mobile.Shopping.HttpAggregator.Infrastructure;

// GrpcExceptionInterceptor 继承自 Interceptor，用于拦截 gRPC 的异步调用。
public class GrpcExceptionInterceptor : Interceptor
{
    // ILogger 用于记录错误日志。
    private readonly ILogger<GrpcExceptionInterceptor> _logger;

    // 构造函数，通过依赖注入提供日志记录器。
    public GrpcExceptionInterceptor(ILogger<GrpcExceptionInterceptor> logger)
    {
        _logger = logger;
    }

    // 重写 AsyncUnaryCall 方法，以拦截 gRPC 的异步调用。
    // 参数说明：
    // TRequest request: gRPC 请求对象。
    // ClientInterceptorContext<TRequest, TResponse> context: gRPC 调用的上下文，包含元数据等信息。
    // AsyncUnaryCallContinuation<TRequest, TResponse> continuation: 下一个调用链处理器。
    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        // 执行实际的 gRPC 调用
        var call = continuation(request, context);

        // 返回一个新的 AsyncUnaryCall 对象，
        // 使用 HandleResponse 方法处理响应中的异常。
        return new AsyncUnaryCall<TResponse>(
            HandleResponse(call.ResponseAsync), // 异常处理后的响应结果
            call.ResponseHeadersAsync,          // 响应头任务
            call.GetStatus,                     // 获取调用状态的方法
            call.GetTrailers,                   // 获取调用尾部数据的方法
            call.Dispose);                      // 资源释放
    }

    // 异步方法，用于等待 gRPC 响应并捕获可能发生的 RpcException 异常。
    private async Task<TResponse> HandleResponse<TResponse>(Task<TResponse> task)
    {
        try
        {
            // 等待 gRPC 响应完成并返回响应数据
            var response = await task;
            return response;
        }
        catch (RpcException e)
        {
            // 捕获 RpcException 异常，记录详细错误日志
            _logger.LogError("Error calling via grpc: {Status} - {Message}", e.Status, e.Message);
            // 返回默认值，保证调用流程不会中断
            return default;
        }
    }
}
