namespace Microsoft.eShopOnContainers.Services.Catalog.API.Extensions;

/// <summary>
/// 提供针对 IEnumerable<T> 的扩展方法，允许在选择过程中捕获异常。
/// </summary>
public static class LinqSelectExtensions
{
    /// <summary>
    /// 对给定的集合进行转换，在转换过程中捕获可能发生的异常，并封装转换结果和异常信息。
    /// </summary>
    /// <typeparam name="TSource">集合中元素的类型</typeparam>
    /// <typeparam name="TResult">转换后结果的类型</typeparam>
    /// <param name="enumerable">输入集合</param>
    /// <param name="selector">转换方法</param>
    /// <returns>
    /// 返回一个 IEnumerable，其中的每个元素都是 SelectTryResult，
    /// 包含原始元素、转换结果以及转换过程中捕获的异常（如果有的话）。
    /// </returns>
    public static IEnumerable<SelectTryResult<TSource, TResult>> SelectTry<TSource, TResult>(
        this IEnumerable<TSource> enumerable, Func<TSource, TResult> selector)
    {
        // 遍历集合中的每个元素
        foreach (TSource element in enumerable)
        {
            SelectTryResult<TSource, TResult> returnedValue;
            try
            {
                // 执行转换操作，如果成功则返回转换结果且异常为 null
                returnedValue = new SelectTryResult<TSource, TResult>(element, selector(element), null);
            }
            catch (Exception ex)
            {
                // 如果转换过程中捕获到异常，则将结果设置为默认值，同时记录异常信息
                returnedValue = new SelectTryResult<TSource, TResult>(element, default(TResult), ex);
            }
            // 生成转换结果
            yield return returnedValue;
        }
    }

    /// <summary>
    /// 在处理 SelectTry 的结果时，如果捕获到异常，使用指定的异常处理方法生成新的结果。
    /// </summary>
    /// <typeparam name="TSource">原始输入元素的类型</typeparam>
    /// <typeparam name="TResult">转换结果的类型</typeparam>
    /// <param name="enumerable">SelectTry 的结果集合</param>
    /// <param name="exceptionHandler">异常处理函数，仅接受 Exception 作为参数</param>
    /// <returns>返回处理后的结果集合</returns>
    public static IEnumerable<TResult> OnCaughtException<TSource, TResult>(
        this IEnumerable<SelectTryResult<TSource, TResult>> enumerable, Func<Exception, TResult> exceptionHandler)
    {
        // 对每个转换结果进行判断，如果没有异常则返回原始转换结果，
        // 否则调用异常处理函数生成新的结果
        return enumerable.Select(x => x.CaughtException == null ? x.Result : exceptionHandler(x.CaughtException));
    }

    /// <summary>
    /// 在处理 SelectTry 的结果时，如果捕获到异常，使用指定的异常处理方法生成新的结果。
    /// </summary>
    /// <typeparam name="TSource">原始输入元素的类型</typeparam>
    /// <typeparam name="TResult">转换结果的类型</typeparam>
    /// <param name="enumerable">SelectTry 的结果集合</param>
    /// <param name="exceptionHandler">异常处理函数，接受原始元素和 Exception 作为参数</param>
    /// <returns>返回处理后的结果集合</returns>
    public static IEnumerable<TResult> OnCaughtException<TSource, TResult>(
        this IEnumerable<SelectTryResult<TSource, TResult>> enumerable, Func<TSource, Exception, TResult> exceptionHandler)
    {
        // 相似地，对每个结果判断异常情况，若存在异常则传递原始元素和异常信息到处理函数
        return enumerable.Select(x => x.CaughtException == null ? x.Result : exceptionHandler(x.Source, x.CaughtException));
    }

    /// <summary>
    /// 封装转换结果及可能在转换过程中捕获的异常。
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
        /// <param name="exception">捕获的异常（如果有）</param>
        internal SelectTryResult(TSource source, TResult result, Exception exception)
        {
            Source = source;
            Result = result;
            CaughtException = exception;
        }

        /// <summary>
        /// 得到或设置原始输入元素
        /// </summary>
        public TSource Source { get; private set; }

        /// <summary>
        /// 得到或设置转换后的结果
        /// </summary>
        public TResult Result { get; private set; }

        /// <summary>
        /// 获取或设置在转换过程中捕获的异常（若没有发生异常，则为 null）
        /// </summary>
        public Exception CaughtException { get; private set; }
    }
}
