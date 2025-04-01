using Microsoft.eShopOnContainers.Services.Ordering.Domain.SeedWork;

namespace Microsoft.eShopOnContainers.Services.Ordering.Domain.AggregatesModel.OrderAggregate;

/// <summary>
/// 地址值对象，表示订单中的地址信息。
/// 作为值对象，它的相等性比较基于所有的属性，且通常是不变的。
/// </summary>
public class Address : ValueObject
{
    // 街道地址
    public string Street { get; private set; }
    // 城市
    public string City { get; private set; }
    // 州或省份
    public string State { get; private set; }
    // 国家
    public string Country { get; private set; }
    // 邮政编码
    public string ZipCode { get; private set; }

    // 无参构造函数，通常用于ORM或反射等场景
    public Address() { }

    // 带参构造函数，用于创建包含所有地址信息的实体
    public Address(string street, string city, string state, string country, string zipcode)
    {
        Street = street;
        City = city;
        State = state;
        Country = country;
        ZipCode = zipcode;
    }

    // 重写基类ValueObject中由所有属性组成的相等性判断方法
    // 这里依次返回Street, City, State, Country, ZipCode用于比较两个Address对象是否相等
    protected override IEnumerable<object> GetEqualityComponents()
    {
        // 使用yield return逐个返回各属性
        yield return Street;
        yield return City;
        yield return State;
        yield return Country;
        yield return ZipCode;
    }
}
