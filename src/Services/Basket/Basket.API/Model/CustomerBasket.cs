namespace Microsoft.eShopOnContainers.Services.Basket.API.Model;

// 定义购物篮客户类，用于表示指定买家的购物篮
public class CustomerBasket
{
    // 买家的标识（例如用户ID）
    public string BuyerId { get; set; }

    // 存储购物篮中各个购物项的列表，初始化为空列表
    public List<BasketItem> Items { get; set; } = new();

    // 默认构造函数，不做任何初始化操作
    public CustomerBasket()
    {

    }

    // 带参数构造函数，根据传入的customerId初始化BuyerId属性
    public CustomerBasket(string customerId)
    {
        BuyerId = customerId;
    }
}

