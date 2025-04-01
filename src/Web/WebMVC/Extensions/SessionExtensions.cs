/// <summary>
/// 提供ISession接口的扩展方法，用于存储和检索复杂对象
/// </summary>
public static class SessionExtensions
{
    /// <summary>
    /// 将对象序列化为JSON并存储在会话中
    /// </summary>
    /// <param name="session">会话对象</param>
    /// <param name="key">存储键</param>
    /// <param name="value">要存储的对象</param>
    public static void SetObject(this ISession session, string key, object value) =>
        session.SetString(key, JsonSerializer.Serialize(value));

    /// <summary>
    /// 从会话中检索JSON字符串并反序列化为指定类型的对象
    /// </summary>
    /// <typeparam name="T">要反序列化的对象类型</typeparam>
    /// <param name="session">会话对象</param>
    /// <param name="key">存储键</param>
    /// <returns>反序列化后的对象，如果键不存在则返回默认值</returns>
    public static T GetObject<T>(this ISession session, string key)
    {
        var value = session.GetString(key);

        return value == null ? default(T) : JsonSerializer.Deserialize<T>(value, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true // 忽略属性名称的大小写
        });
    }
}

