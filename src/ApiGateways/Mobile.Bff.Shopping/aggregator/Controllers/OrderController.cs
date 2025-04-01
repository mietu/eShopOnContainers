namespace Microsoft.eShopOnContainers.Mobile.Shopping.HttpAggregator.Controllers;

// 定义该控制器的路由前缀为 "api/v1/Order"
[Route("api/v1/[controller]")]
// 要求调用此控制器的客户端必须通过身份验证
[Authorize]
[ApiController]
public class OrderController : ControllerBase
{
    private readonly IBasketService _basketService;
    private readonly IOrderingService _orderingService;

    // 通过构造函数注入 IBasketService 和 IOrderingService 实例
    public OrderController(IBasketService basketService, IOrderingService orderingService)
    {
        _basketService = basketService;
        _orderingService = orderingService;
    }

    // 定义 GET 请求，路由为 api/v1/Order/draft/{basketId}
    [Route("draft/{basketId}")]
    [HttpGet]
    // 如果请求参数无效，将返回 HTTP 400 错误
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    // 成功时返回 OrderData 数据，HTTP 200
    [ProducesResponseType(typeof(OrderData), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<OrderData>> GetOrderDraftAsync(string basketId)
    {
        // 检查 basketId 参数是否为空或空字符串
        if (string.IsNullOrEmpty(basketId))
        {
            // 返回400错误，提示 basketId 无效
            return BadRequest("Need a valid basketid");
        }
        // 根据 basketId 获取购物篮数据，调用 IBasketService 的异步方法
        var basket = await _basketService.GetByIdAsync(basketId);

        // 如果未找到购物篮，则返回400错误，并提示未找到对应的 basketId
        if (basket == null)
        {
            return BadRequest($"No basket found for id {basketId}");
        }

        // 根据获取的购物篮数据构建订单草稿，并返回异步结果
        return await _orderingService.GetOrderDraftAsync(basket);
    }
}
