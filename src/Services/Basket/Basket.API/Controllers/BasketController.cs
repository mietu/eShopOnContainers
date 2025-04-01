namespace Microsoft.eShopOnContainers.Services.Basket.API.Controllers;

/// <summary>
/// 购物篮控制器，提供购物篮相关的API接口
/// 包括获取、更新、删除购物篮以及结账功能
/// </summary>
[Route("api/v1/[controller]")]
[Authorize] // 需要用户授权
[ApiController]
public class BasketController : ControllerBase
{
    private readonly IBasketRepository _repository;    // 购物篮数据仓储接口
    private readonly IIdentityService _identityService; // 用户身份服务接口
    private readonly IEventBus _eventBus;              // 事件总线，用于发布集成事件
    private readonly ILogger<BasketController> _logger; // 日志服务

    /// <summary>
    /// 构造函数，通过依赖注入获取所需服务
    /// </summary>
    /// <param name="logger">日志记录器</param>
    /// <param name="repository">购物篮仓储</param>
    /// <param name="identityService">身份服务</param>
    /// <param name="eventBus">事件总线</param>
    public BasketController(
        ILogger<BasketController> logger,
        IBasketRepository repository,
        IIdentityService identityService,
        IEventBus eventBus)
    {
        _logger = logger;
        _repository = repository;
        _identityService = identityService;
        _eventBus = eventBus;
    }

    /// <summary>
    /// 根据ID获取购物篮
    /// </summary>
    /// <param name="id">购物篮ID（通常是用户ID）</param>
    /// <returns>购物篮对象，若不存在则返回空购物篮</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CustomerBasket), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<CustomerBasket>> GetBasketByIdAsync(string id)
    {
        var basket = await _repository.GetBasketAsync(id);

        return Ok(basket ?? new CustomerBasket(id));
    }

    /// <summary>
    /// 更新购物篮
    /// </summary>
    /// <param name="value">新的购物篮数据</param>
    /// <returns>更新后的购物篮</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CustomerBasket), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<CustomerBasket>> UpdateBasketAsync([FromBody] CustomerBasket value)
    {
        return Ok(await _repository.UpdateBasketAsync(value));
    }

    /// <summary>
    /// 购物篮结账
    /// 将处理购买流程并发布结账事件通知订单服务
    /// </summary>
    /// <param name="basketCheckout">结账相关信息，包含支付和配送详情</param>
    /// <param name="requestId">请求ID，用于幂等性处理</param>
    /// <returns>接受结果，表示结账请求已处理</returns>
    [Route("checkout")]
    [HttpPost]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult> CheckoutAsync([FromBody] BasketCheckout basketCheckout, [FromHeader(Name = "x-requestid")] string requestId)
    {
        var userId = _identityService.GetUserIdentity();

        // 处理请求ID，确保请求可追踪
        basketCheckout.RequestId = (Guid.TryParse(requestId, out Guid guid) && guid != Guid.Empty) ?
            guid : basketCheckout.RequestId;

        // 获取用户购物篮
        var basket = await _repository.GetBasketAsync(userId);

        // 购物篮不存在，返回错误
        if (basket == null)
        {
            return BadRequest();
        }

        // 获取用户名
        var userName = this.HttpContext.User.FindFirst(x => x.Type == ClaimTypes.Name).Value;

        // 创建用户结账接受事件
        var eventMessage = new UserCheckoutAcceptedIntegrationEvent(userId, userName, basketCheckout.City, basketCheckout.Street,
            basketCheckout.State, basketCheckout.Country, basketCheckout.ZipCode, basketCheckout.CardNumber, basketCheckout.CardHolderName,
            basketCheckout.CardExpiration, basketCheckout.CardSecurityNumber, basketCheckout.CardTypeId, basketCheckout.Buyer, basketCheckout.RequestId, basket);

        // 购物篮结账后，发送集成事件到订单API
        // 将购物篮转换为订单并进行订单创建流程
        try
        {
            _eventBus.Publish(eventMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ERROR Publishing integration event: {IntegrationEventId} from {AppName}", eventMessage.Id, Program.AppName);

            throw;
        }

        return Accepted();
    }

    /// <summary>
    /// 根据ID删除购物篮
    /// </summary>
    /// <param name="id">购物篮ID（通常是用户ID）</param>
    /// <returns>无返回内容的任务</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
    public async Task DeleteBasketByIdAsync(string id)
    {
        await _repository.DeleteBasketAsync(id);
    }
}
