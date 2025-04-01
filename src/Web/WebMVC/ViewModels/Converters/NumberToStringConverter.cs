namespace Microsoft.eShopOnContainers.WebMVC.ViewModels;

/// <summary>
/// 自定义 JSON 转换器，用于处理 JSON 数据中数字到字符串的转换
/// 当 JSON 反序列化时，可以将数字值转换为字符串类型
/// </summary>
public class NumberToStringConverter : JsonConverter<string>
{
    /// <summary>
    /// 从 JSON 读取数据并转换为字符串
    /// </summary>
    /// <param name="reader">UTF-8 JSON 读取器</param>
    /// <param name="typeToConvert">要转换的目标类型</param>
    /// <param name="options">JSON 序列化选项</param>
    /// <returns>转换后的字符串值</returns>
    /// <exception cref="JsonException">当 JSON 令牌类型既不是数字也不是字符串时抛出</exception>
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            // 如果 JSON 值是数字类型，则读取为整数并转换为字符串
            var numberValue = reader.GetInt32();
            return numberValue.ToString();
        }
        else if (reader.TokenType == JsonTokenType.String)
        {
            // 如果 JSON 值已经是字符串类型，则直接返回
            return reader.GetString();
        }
        else
        {
            // 如果 JSON 值既不是数字也不是字符串，则抛出异常
            throw new JsonException();
        }
    }

    /// <summary>
    /// 将字符串值写入 JSON
    /// </summary>
    /// <param name="writer">JSON 写入器</param>
    /// <param name="value">要写入的字符串值</param>
    /// <param name="options">JSON 序列化选项</param>
    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        // 将提供的值作为字符串写入 JSON
        writer.WriteStringValue(value);
    }
}
