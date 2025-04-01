namespace Microsoft.eShopOnContainers.BuildingBlocks.EventBus.Extensions;

public static class GenericTypeExtensions
{
    /// <summary>
    /// 获取类型的名称，如果是泛型类型，则返回格式化后的泛型名称。
    /// </summary>
    /// <param name="type">需要获取名称的类型</param>
    /// <returns>类型的名称字符串</returns>
    public static string GetGenericTypeName(this Type type)
    {
        // 定义存储最终类型名称的变量
        string typeName;

        // 如果该类型是泛型类型
        if (type.IsGenericType)
        {
            // 获取所有泛型参数的名称，并用逗号隔开
            var genericTypes = string.Join(",", type.GetGenericArguments().Select(t => t.Name).ToArray());
            // 去除类型名称中的 ` 符号及之后的字符，然后格式化为泛型名称
            typeName = $"{type.Name.Remove(type.Name.IndexOf('`'))}<{genericTypes}>";
        }
        else
        {
            // 如果不是泛型类型，则直接使用类型名称
            typeName = type.Name;
        }

        // 返回处理后的类型名称
        return typeName;
    }

    /// <summary>
    /// 获取对象的类型名称，如果是泛型类型，则返回格式化后的泛型名称。
    /// </summary>
    /// <param name="object">需要获取类型名称的对象</param>
    /// <returns>对象类型的名称字符串</returns>
    public static string GetGenericTypeName(this object @object)
    {
        // 调用上述扩展方法，基于对象的类型获取名称
        return @object.GetType().GetGenericTypeName();
    }
}
