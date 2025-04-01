namespace Microsoft.eShopOnContainers.Services.Ordering.Domain.SeedWork;

// 抽象类，用以创建可比较和枚举的对象，其特点是具有 Id 和 Name 属性。
public abstract class Enumeration : IComparable
{
    // 存储枚举项的名称
    public string Name { get; private set; }
    // 存储枚举项的标识符
    public int Id { get; private set; }

    // 构造函数，初始化 Id 和 Name
    protected Enumeration(int id, string name) => (Id, Name) = (id, name);

    // 重写 ToString 方法，返回名称
    public override string ToString() => Name;

    // 获取指定枚举类型 T 中的所有枚举项
    public static IEnumerable<T> GetAll<T>() where T : Enumeration =>
        // 通过反射获取所有公共、静态、在此类中声明的字段，并转换为枚举类型
        typeof(T).GetFields(BindingFlags.Public |
                            BindingFlags.Static |
                            BindingFlags.DeclaredOnly)
                 .Select(f => f.GetValue(null))
                 .Cast<T>();

    // 重写 Equals 方法，根据类型和 Id 判断两个枚举项是否相等
    public override bool Equals(object obj)
    {
        // 如果 obj 不是 Enumeration 类型，返回 false
        if (obj is not Enumeration otherValue)
        {
            return false;
        }

        // 检查类型是否匹配以及 Id 是否相同
        var typeMatches = GetType().Equals(obj.GetType());
        var valueMatches = Id.Equals(otherValue.Id);

        return typeMatches && valueMatches;
    }

    // 重写 GetHashCode 方法，基于 Id 返回 hash code
    public override int GetHashCode() => Id.GetHashCode();

    // 计算两个枚举项之间 Id 的绝对差值
    public static int AbsoluteDifference(Enumeration firstValue, Enumeration secondValue)
    {
        var absoluteDifference = Math.Abs(firstValue.Id - secondValue.Id);
        return absoluteDifference;
    }

    // 根据整数值找到对应的枚举项
    public static T FromValue<T>(int value) where T : Enumeration
    {
        // 调用私有的 Parse 方法，使用 predicate 匹配 Id
        var matchingItem = Parse<T, int>(value, "value", item => item.Id == value);
        return matchingItem;
    }

    // 根据名称找到对应的枚举项
    public static T FromDisplayName<T>(string displayName) where T : Enumeration
    {
        // 调用私有的 Parse 方法，使用 predicate 匹配 Name
        var matchingItem = Parse<T, string>(displayName, "display name", item => item.Name == displayName);
        return matchingItem;
    }

    // 通用解析方法，根据传入的 predicate 返回匹配的枚举项，如果未找到则抛出异常
    private static T Parse<T, K>(K value, string description, Func<T, bool> predicate) where T : Enumeration
    {
        // 遍历所有 T 类型的枚举项，返回第一个匹配 predicate 的项
        var matchingItem = GetAll<T>().FirstOrDefault(predicate);

        // 如果没有找到匹配的项，则抛出异常
        if (matchingItem == null)
            throw new InvalidOperationException($"'{value}' is not a valid {description} in {typeof(T)}");

        return matchingItem;
    }

    // 实现 IComparable 接口，根据 Id 进行比较
    public int CompareTo(object other) => Id.CompareTo(((Enumeration)other).Id);
}
