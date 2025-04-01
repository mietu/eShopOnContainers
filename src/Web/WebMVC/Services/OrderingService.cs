namespace Microsoft.eShopOnContainers.WebMVC.Services;

using Microsoft.eShopOnContainers.WebMVC.ViewModels;

/// <summary>
/// 订单服务类，负责处理与订单相关的操作，包括获取、取消和发货订单，
/// 以及在用户信息和订单之间进行数据映射。
/// </summary>
public class OrderingService : IOrderingService
{
    private HttpClient _httpClient;
    private readonly string _remoteServiceBaseUrl;
    private readonly IOptions<AppSettings> _settings;

    /// <summary>
    /// 构造函数，初始化订单服务及其依赖项
    /// </summary>
    /// <param name="httpClient">用于发送HTTP请求的客户端</param>
    /// <param name="settings">应用程序配置</param>
    public OrderingService(HttpClient httpClient, IOptions<AppSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings;

        _remoteServiceBaseUrl = $"{settings.Value.PurchaseUrl}/o/api/v1/orders";
    }

    /// <summary>
    /// 获取指定ID的订单详情
    /// </summary>
    /// <param name="user">当前用户</param>
    /// <param name="id">订单ID</param>
    /// <returns>订单详情对象</returns>
    async public Task<Order> GetOrder(ApplicationUser user, string id)
    {
        var uri = API.Order.GetOrder(_remoteServiceBaseUrl, id);

        var responseString = await _httpClient.GetStringAsync(uri);

        var response = JsonSerializer.Deserialize<Order>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return response;
    }

    /// <summary>
    /// 获取当前用户的所有订单
    /// </summary>
    /// <param name="user">当前用户</param>
    /// <returns>用户的订单列表</returns>
    async public Task<List<Order>> GetMyOrders(ApplicationUser user)
    {
        var uri = API.Order.GetAllMyOrders(_remoteServiceBaseUrl);

        var responseString = await _httpClient.GetStringAsync(uri);

        var response = JsonSerializer.Deserialize<List<Order>>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return response;
    }

    /// <summary>
    /// 取消指定ID的订单
    /// </summary>
    /// <param name="orderId">需要取消的订单ID</param>
    /// <returns>表示异步操作的任务</returns>
    /// <exception cref="Exception">当取消订单操作失败时抛出</exception>
    async public Task CancelOrder(string orderId)
    {
        var order = new OrderDTO()
        {
            OrderNumber = orderId
        };

        var uri = API.Order.CancelOrder(_remoteServiceBaseUrl);
        var orderContent = new StringContent(JsonSerializer.Serialize(order), System.Text.Encoding.UTF8, "application/json");

        var response = await _httpClient.PutAsync(uri, orderContent);

        if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
        {
            throw new Exception("Error cancelling order, try later.");
        }

        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// 发货指定ID的订单
    /// </summary>
    /// <param name="orderId">需要发货的订单ID</param>
    /// <returns>表示异步操作的任务</returns>
    /// <exception cref="Exception">当发货操作失败时抛出</exception>
    async public Task ShipOrder(string orderId)
    {
        var order = new OrderDTO()
        {
            OrderNumber = orderId
        };

        var uri = API.Order.ShipOrder(_remoteServiceBaseUrl);
        var orderContent = new StringContent(JsonSerializer.Serialize(order), System.Text.Encoding.UTF8, "application/json");

        var response = await _httpClient.PutAsync(uri, orderContent);

        if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
        {
            throw new Exception("Error in ship order process, try later.");
        }

        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// 将原始订单的用户信息覆盖到目标订单中
    /// </summary>
    /// <param name="original">包含源用户信息的订单</param>
    /// <param name="destination">需要更新用户信息的目标订单</param>
    public void OverrideUserInfoIntoOrder(Order original, Order destination)
    {
        destination.City = original.City;
        destination.Street = original.Street;
        destination.State = original.State;
        destination.Country = original.Country;
        destination.ZipCode = original.ZipCode;

        destination.CardNumber = original.CardNumber;
        destination.CardHolderName = original.CardHolderName;
        destination.CardExpiration = original.CardExpiration;
        destination.CardSecurityNumber = original.CardSecurityNumber;
    }

    /// <summary>
    /// 将用户个人信息映射到订单对象中
    /// </summary>
    /// <param name="user">包含用户信息的对象</param>
    /// <param name="order">需要填充用户信息的订单</param>
    /// <returns>更新后的订单对象</returns>
    public Order MapUserInfoIntoOrder(ApplicationUser user, Order order)
    {
        order.City = user.City;
        order.Street = user.Street;
        order.State = user.State;
        order.Country = user.Country;
        order.ZipCode = user.ZipCode;

        order.CardNumber = user.CardNumber;
        order.CardHolderName = user.CardHolderName;
        order.CardExpiration = new DateTime(int.Parse("20" + user.Expiration.Split('/')[1]), int.Parse(user.Expiration.Split('/')[0]), 1);
        order.CardSecurityNumber = user.SecurityNumber;

        return order;
    }

    /// <summary>
    /// 将订单信息映射到购物篮DTO对象中
    /// </summary>
    /// <param name="order">订单信息</param>
    /// <returns>转换后的购物篮DTO对象</returns>
    public BasketDTO MapOrderToBasket(Order order)
    {
        order.CardExpirationApiFormat();

        return new BasketDTO()
        {
            City = order.City,
            Street = order.Street,
            State = order.State,
            Country = order.Country,
            ZipCode = order.ZipCode,
            CardNumber = order.CardNumber,
            CardHolderName = order.CardHolderName,
            CardExpiration = order.CardExpiration,
            CardSecurityNumber = order.CardSecurityNumber,
            CardTypeId = 1,
            Buyer = order.Buyer,
            RequestId = order.RequestId
        };
    }
}
