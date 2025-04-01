namespace Microsoft.eShopOnContainers.Web.Shopping.HttpAggregator.Models;

// BasketData 类用于封装购物篮的相关数据
public class BasketData
{
    // BuyerId 属性标识购物篮所属的购买者
    public string BuyerId { get; set; }

    // Items 属性存储购物篮中所有的商品项，初始化为一个空列表
    public List<BasketDataItem> Items { get; set; } = new();

    // 默认构造函数，允许创建一个空的 BasketData 实例
    public BasketData()
    {
    }

    // 带 BuyerId 参数的构造函数，用于在实例化时设置购买者标识符
    public BasketData(string buyerId)
    {
        BuyerId = buyerId;
    }
}

