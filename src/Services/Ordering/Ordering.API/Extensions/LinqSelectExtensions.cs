namespace Microsoft.eShopOnContainers.Services.Ordering.API.Extensions;

/// <summary>
/// 提供对 IEnumerable 集合中元素进行安全转换的扩展方法。
/// </summary>
public static class LinqSelectExtensions
{
    /// <summary>
    /// 尝试对集合中的每个元素使用 selector 函数进行转换。
    /// 对于转换过程中可能抛出异常的元素，会捕获异常并将其包含在返回结果中。
    /// </summary>
    /// <typeparam name="TSource">输入元素类型</typeparam>
    /// <typeparam name="TResult">输出元素类型</typeparam>
    /// <param name="enumerable">源集合</param>
    /// <param name="selector">转换函数</param>
    /// <returns>包含转换结果和捕获异常信息的集合</returns>
    public static IEnumerable<SelectTryResult<TSource, TResult>> SelectTry<TSource, TResult>(this IEnumerable<TSource> enumerable, Func<TSource, TResult> selector)
    {
        // 遍历列表中每个元素
        foreach (TSource element in enumerable)
        {
            // 定义返回的结果
            SelectTryResult<TSource, TResult> returnedValue;
            try
            {
                // 尝试对元素进行转换，如果成功则传入转换结果，并将异常设置为 null
                returnedValue = new SelectTryResult<TSource, TResult>(element, selector(element), null);
            }
            catch (Exception ex)
            {
                // 如果转换过程中发生异常，捕获异常，并将结果设置为默认值
                returnedValue = new SelectTryResult<TSource, TResult>(element, default(TResult), ex);
            }
            // 返回带有转换结果和异常信息（如果有）的对象
            yield return returnedValue;
        }
    }

    /// <summary>
    /// 当捕获到转换异常时，使用提供的 exceptionHandler 对异常进行处理，
    /// 并返回处理结果；否则返回转换后的结果。
    /// </summary>
    /// <typeparam name="TSource">输入元素类型</typeparam>
    /// <typeparam name="TResult">最终输出类型</typeparam>
    /// <param name="enumerable">包含转换结果和异常信息的集合</param>
    /// <param name="exceptionHandler">处理异常的委托，仅接受 Exception 参数</param>
    /// <returns>使用异常处理函数解决异常后的结果集合</returns>
    public static IEnumerable<TResult> OnCaughtException<TSource, TResult>(this IEnumerable<SelectTryResult<TSource, TResult>> enumerable, Func<Exception, TResult> exceptionHandler)
    {
        // 对每个转换结果进行判断，如果存在异常则调用 exceptionHandler，否则返回转换结果
        return enumerable.Select(x => x.CaughtException == null ? x.Result : exceptionHandler(x.CaughtException));
    }

    /// <summary>
    /// 当捕获到转换异常时，使用提供的 exceptionHandler 对异常进行处理，
    /// 并返回处理结果；否则返回转换后的结果。此方法的 exceptionHandler 同时接受原始元素和异常信息。
    /// </summary>
    /// <typeparam name="TSource">输入元素类型</typeparam>
    /// <typeparam name="TResult">最终输出类型</typeparam>
    /// <param name="enumerable">包含转换结果和异常信息的集合</param>
    /// <param name="exceptionHandler">处理异常的委托，接受原始元素和 Exception 参数</param>
    /// <returns>使用异常处理函数解决异常后的结果集合</returns>
    public static IEnumerable<TResult> OnCaughtException<TSource, TResult>(this IEnumerable<SelectTryResult<TSource, TResult>> enumerable, Func<TSource, Exception, TResult> exceptionHandler)
    {
        // 对每个转换结果进行判断，如果存在异常则调用 exceptionHandler（传入原始元素和异常），否则返回转换结果
        return enumerable.Select(x => x.CaughtException == null ? x.Result : exceptionHandler(x.Source, x.CaughtException));
    }

    /// <summary>
    /// 包装了转换结果及可能在转换过程中捕获的异常。
    /// </summary>
    /// <typeparam name="TSource">原始元素类型</typeparam>
    /// <typeparam name="TResult">转换结果类型</typeparam>
    public class SelectTryResult<TSource, TResult>
    {
        /// <summary>
        /// 内部构造函数，用于初始化转换结果和相关信息。
        /// </summary>
        /// <param name="source">原始元素</param>
        /// <param name="result">转换后的结果</param>
        /// <param name="exception">捕获的异常</param>
        internal SelectTryResult(TSource source, TResult result, Exception exception)
        {
            Source = source;
            Result = result;
            CaughtException = exception;
        }

        /// <summary>
        /// 得到原始输入元素
        /// </summary>
        public TSource Source { get; private set; }
        /// <summary>
        /// 得到转换后的结果
        /// </summary>
        public TResult Result { get; private set; }
        /// <summary>
        /// 获取在转换过程中捕获的异常（若没有发生异常，则为 null）
        /// </summary>
        public Exception CaughtException { get; private set; }
    }
}
