namespace Microsoft.eShopOnContainers.Services.Ordering.Domain.Seedwork;

// 抽象实体模型，所有实体均继承此类
public abstract class Entity
{
    // 用于缓存计算的哈希码
    int? _requestedHashCode;
    // 实体唯一标识符的私有字段
    int _Id;

    // 公开的标识符属性，实体标识符
    public virtual int Id
    {
        get
        {
            return _Id;
        }
        // 保护级别的设置器，只有子类或本类可以赋值
        protected set
        {
            _Id = value;
        }
    }

    // 存储领域事件的集合（通知）
    private List<INotification> _domainEvents;
    // 只读属性，对外暴露领域事件
    public IReadOnlyCollection<INotification> DomainEvents => _domainEvents?.AsReadOnly();

    // 添加一个领域事件，如果集合为空则初始化后添加
    public void AddDomainEvent(INotification eventItem)
    {
        _domainEvents = _domainEvents ?? new List<INotification>();
        _domainEvents.Add(eventItem);
    }

    // 移除指定的领域事件
    public void RemoveDomainEvent(INotification eventItem)
    {
        _domainEvents?.Remove(eventItem);
    }

    // 清除所有添加的领域事件
    public void ClearDomainEvents()
    {
        _domainEvents?.Clear();
    }

    // 判断实体是否为瞬态（未持久化），通过判断Id是否为默认值
    public bool IsTransient()
    {
        return this.Id == default(int);
    }

    // 重写Equals方法，用于比较实体相等性
    public override bool Equals(object obj)
    {
        // 如果对象为空或不是实体，则返回false
        if (obj == null || !(obj is Entity))
            return false;

        // 如果两个对象引用相同，则返回true
        if (object.ReferenceEquals(this, obj))
            return true;

        // 判断两个对象是否属于同一类型
        if (this.GetType() != obj.GetType())
            return false;

        Entity item = (Entity)obj;

        // 如果任一实体为瞬态，则认为不相等（因为可能未持久化）
        if (item.IsTransient() || this.IsTransient())
            return false;
        else
            // 否则通过Id是否相等来判断实体相等性
            return item.Id == this.Id;
    }

    // 重写GetHashCode方法，确保实体具有合适的哈希码
    public override int GetHashCode()
    {
        // 如果实体已持久化，则通过Id计算哈希码
        if (!IsTransient())
        {
            if (!_requestedHashCode.HasValue)
                // 使用Id的哈希码与31异或得到更广泛的哈希分布
                _requestedHashCode = this.Id.GetHashCode() ^ 31;

            return _requestedHashCode.Value;
        }
        else
            // 瞬态实体使用基类的哈希码实现
            return base.GetHashCode();

    }

    // 重载等于运算符，基于重写的Equals方法
    public static bool operator ==(Entity left, Entity right)
    {
        if (object.Equals(left, null))
            return (object.Equals(right, null)) ? true : false;
        else
            return left.Equals(right);
    }

    // 重载不等于运算符，与等于运算符相反
    public static bool operator !=(Entity left, Entity right)
    {
        return !(left == right);
    }
}
