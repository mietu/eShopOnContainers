namespace Microsoft.eShopOnContainers.Services.Ordering.Domain.SeedWork;

public abstract class ValueObject
{
    // EqualOperator 用于判断两个值对象是否相等。
    // 若其中一个为 null 而另一个不为 null，则返回 false；
    // 若两个均为 null，则返回 true；否则调用 Equals 方法比较。
    protected static bool EqualOperator(ValueObject left, ValueObject right)
    {
        // 使用异或操作符检查是否只有一个对象为 null
        if (ReferenceEquals(left, null) ^ ReferenceEquals(right, null))
        {
            return false;
        }
        // 如果 left 为 null，两个都是 null，返回 true；否则比较它们的内容
        return ReferenceEquals(left, null) || left.Equals(right);
    }

    // NotEqualOperator 是 EqualOperator 的反向逻辑
    protected static bool NotEqualOperator(ValueObject left, ValueObject right)
    {
        return !(EqualOperator(left, right));
    }

    // GetEqualityComponents 方法用于获取参与相等性比较的组件集合
    // 派生类需要实现此方法，返回所有对比值对象相等性的重要属性或字段
    protected abstract IEnumerable<object> GetEqualityComponents();

    // 重写 Equals 方法用于基于 GetEqualityComponents 的序列比较来判断两个值对象是否相等
    public override bool Equals(object obj)
    {
        // 如果其他对象为 null 或非当前相同类型，则返回 false
        if (obj == null || obj.GetType() != GetType())
        {
            return false;
        }

        var other = (ValueObject)obj;
        // 使用 GetEqualityComponents 获取组成部分并依次比较
        return this.GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    // 重写 GetHashCode 方法，根据所有参与比较的组件生成哈希码
    public override int GetHashCode()
    {
        return GetEqualityComponents()
            // 对每个组件若不为 null 则获取哈希码，否则为 0
            .Select(x => x != null ? x.GetHashCode() : 0)
            // 使用 XOR 运算组合所有组件的哈希码
            .Aggregate((x, y) => x ^ y);
    }

    // GetCopy 方法用于返回当前值对象的浅拷贝
    public ValueObject GetCopy()
    {
        return this.MemberwiseClone() as ValueObject;
    }
}
