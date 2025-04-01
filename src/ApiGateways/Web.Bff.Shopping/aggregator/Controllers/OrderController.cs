namespace Microsoft.eShopOnContainers.Web.Shopping.HttpAggregator.Controllers;

// 指定控制器的路由前缀为 "api/v1/Order"，并启用授权验证以及API控制器行为
[Route("api/v1/[controller]")]
[Authorize]
[ApiController]
public class OrderController : ControllerBase
{
    // 依赖注入篮子服务，用于获取篮子数据
    private readonly IBasketService _basketService;
    // 依赖注入订单服务，用于生成订单草稿
    private readonly IOrderingService _orderingService;

    // 构造函数，通过依赖注入初始化服务实例
    public OrderController(IBasketService basketService, IOrderingService orderingService)
    {
        _basketService = basketService;
        _orderingService = orderingService;
    }

    /// <summary>
    /// 根据篮子ID获取订单草稿
    /// </summary>
    /// <param name="basketId">篮子ID</param>
    /// <returns>返回订单草稿数据(OrderData)</returns>
    [Route("draft/{basketId}")]
    [HttpGet]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(OrderData), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<OrderData>> GetOrderDraftAsync(string basketId)
    {
        // 检查传入的basketId是否为空或空白
        if (string.IsNullOrWhiteSpace(basketId))
        {
            return BadRequest("Need a valid basketid");
        }

        // 从篮子服务中获取对应的篮子数据
        var basket = await _basketService.GetByIdAsync(basketId);

        // 若未找到篮子数据，则返回错误
        if (basket == null)
        {
            return BadRequest($"No basket found for id {basketId}");
        }

        // 调用订单服务生成订单草稿，并返回生成的订单数据
        return await _orderingService.GetOrderDraftAsync(basket);
    }
}
