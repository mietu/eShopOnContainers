namespace Microsoft.eShopOnContainers.Services.Ordering.API.Controllers;

using Microsoft.eShopOnContainers.BuildingBlocks.EventBus.Extensions;
using Microsoft.eShopOnContainers.Services.Ordering.API.Application.Commands;
using Microsoft.eShopOnContainers.Services.Ordering.API.Application.Queries;
using Microsoft.eShopOnContainers.Services.Ordering.API.Infrastructure.Services;

/// <summary>
/// 订单控制器：处理订单相关的 API 请求，包括取消订单、发货订单、获取订单详情、获取订单列表、获取信用卡类型及创建订单草稿等操作。
/// </summary>
[Route("api/v1/[controller]")]
[Authorize]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IOrderQueries _orderQueries;
    private readonly IIdentityService _identityService;
    private readonly ILogger<OrdersController> _logger;

    /// <summary>
    /// 构造函数，注入所需依赖项。
    /// </summary>
    /// <param name="mediator">Mediator 对象用于发送命令和查询</param>
    /// <param name="orderQueries">订单查询服务</param>
    /// <param name="identityService">身份认证服务</param>
    /// <param name="logger">日志记录器</param>
    public OrdersController(
        IMediator mediator,
        IOrderQueries orderQueries,
        IIdentityService identityService,
        ILogger<OrdersController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _orderQueries = orderQueries ?? throw new ArgumentNullException(nameof(orderQueries));
        _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 取消订单接口
    /// 通过请求头中的 x-requestid 参数确保命令的幂等性
    /// </summary>
    /// <param name="command">取消订单命令对象</param>
    /// <param name="requestId">请求唯一标识</param>
    /// <returns>返回操作结果：成功则返回 Ok，否则返回 BadRequest</returns>
    [Route("cancel")]
    [HttpPut]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> CancelOrderAsync([FromBody] CancelOrderCommand command, [FromHeader(Name = "x-requestid")] string requestId)
    {
        bool commandResult = false;

        // 验证 requestId 是否为有效的 Guid 值
        if (Guid.TryParse(requestId, out Guid guid) && guid != Guid.Empty)
        {
            // 使用 IdentifiedCommand 包装命令，确保幂等性
            var requestCancelOrder = new IdentifiedCommand<CancelOrderCommand, bool>(command, guid);

            // 记录日志，方便追踪命令发送的细节
            _logger.LogInformation(
                "----- Sending command: {CommandName} - {IdProperty}: {CommandId} ({@Command})",
                requestCancelOrder.GetGenericTypeName(),
                nameof(requestCancelOrder.Command.OrderNumber),
                requestCancelOrder.Command.OrderNumber,
                requestCancelOrder);

            // 发送命令并等待处理结果
            commandResult = await _mediator.Send(requestCancelOrder);
        }

        // 如果命令处理失败，则返回 BadRequest
        if (!commandResult)
        {
            return BadRequest();
        }

        return Ok();
    }

    /// <summary>
    /// 发货订单接口
    /// 通过请求头中的 x-requestid 参数确保命令的幂等性
    /// </summary>
    /// <param name="command">发货订单命令对象</param>
    /// <param name="requestId">请求唯一标识</param>
    /// <returns>返回操作结果：成功则返回 Ok，否则返回 BadRequest</returns>
    [Route("ship")]
    [HttpPut]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> ShipOrderAsync([FromBody] ShipOrderCommand command, [FromHeader(Name = "x-requestid")] string requestId)
    {
        bool commandResult = false;

        // 验证 requestId 是否为有效的 Guid 值
        if (Guid.TryParse(requestId, out Guid guid) && guid != Guid.Empty)
        {
            var requestShipOrder = new IdentifiedCommand<ShipOrderCommand, bool>(command, guid);

            // 记录发货命令的日志
            _logger.LogInformation(
                "----- Sending command: {CommandName} - {IdProperty}: {CommandId} ({@Command})",
                requestShipOrder.GetGenericTypeName(),
                nameof(requestShipOrder.Command.OrderNumber),
                requestShipOrder.Command.OrderNumber,
                requestShipOrder);

            // 发送命令并等待处理结果
            commandResult = await _mediator.Send(requestShipOrder);
        }

        // 如果命令处理失败，则返回 BadRequest
        if (!commandResult)
        {
            return BadRequest();
        }

        return Ok();
    }

    /// <summary>
    /// 根据订单Id获取特定订单的详细信息。
    /// </summary>
    /// <param name="orderId">订单Id，必须为整型</param>
    /// <returns>返回订单对象，若未找到则返回 NotFound</returns>
    [Route("{orderId:int}")]
    [HttpGet]
    [ProducesResponseType(typeof(Order), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult> GetOrderAsync(int orderId)
    {
        try
        {
            // 注意：可以使用 GetOrderByIdQuery 进行查询，此处直接调用 _orderQueries
            var order = await _orderQueries.GetOrderAsync(orderId);
            return Ok(order);
        }
        catch
        {
            // 捕捉异常并返回 NotFound
            return NotFound();
        }
    }

    /// <summary>
    /// 获取当前用户所有订单的概要列表。
    /// </summary>
    /// <returns>返回订单摘要集合</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrderSummary>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<IEnumerable<OrderSummary>>> GetOrdersAsync()
    {
        // 获取当前用户的身份标识，将其转换为 GUID 使用
        var userid = _identityService.GetUserIdentity();
        var orders = await _orderQueries.GetOrdersFromUserAsync(Guid.Parse(userid));
        return Ok(orders);
    }

    /// <summary>
    /// 获取可用的信用卡类型列表，通常用于订单支付等场景。
    /// </summary>
    /// <returns>返回信用卡类型集合</returns>
    [Route("cardtypes")]
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CardType>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<IEnumerable<CardType>>> GetCardTypesAsync()
    {
        var cardTypes = await _orderQueries.GetCardTypesAsync();
        return Ok(cardTypes);
    }

    /// <summary>
    /// 根据购物车数据创建订单草稿。
    /// 该草稿订单用于后续订单确认和支付流程中展示和操作。
    /// </summary>
    /// <param name="createOrderDraftCommand">创建订单草稿所需命令对象，包含买家Id和购物车项数据</param>
    /// <returns>返回订单草稿数据传输对象 OrderDraftDTO</returns>
    [Route("draft")]
    [HttpPost]
    public async Task<ActionResult<OrderDraftDTO>> CreateOrderDraftFromBasketDataAsync([FromBody] CreateOrderDraftCommand createOrderDraftCommand)
    {
        // 记录创建订单草稿的命令日志信息
        _logger.LogInformation(
            "----- Sending command: {CommandName} - {IdProperty}: {CommandId} ({@Command})",
            createOrderDraftCommand.GetGenericTypeName(),
            nameof(createOrderDraftCommand.BuyerId),
            createOrderDraftCommand.BuyerId,
            createOrderDraftCommand);

        // 发送命令并返回草稿订单对象
        return await _mediator.Send(createOrderDraftCommand);
    }
}
