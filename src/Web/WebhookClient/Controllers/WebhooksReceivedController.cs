namespace WebhookClient.Controllers;

/// <summary>
/// 处理接收到的Webhook请求的控制器
/// </summary>
[ApiController]
[Route("webhook-received")]
public class WebhooksReceivedController : Controller
{
    // 应用程序配置
    private readonly Settings _settings;
    // 日志记录器
    private readonly ILogger _logger;
    // Webhook数据仓库
    private readonly IHooksRepository _hooksRepository;

    /// <summary>
    /// 构造函数，通过依赖注入初始化控制器所需依赖
    /// </summary>
    /// <param name="settings">应用程序配置</param>
    /// <param name="logger">日志记录器</param>
    /// <param name="hooksRepository">Webhook数据仓库</param>
    public WebhooksReceivedController(IOptions<Settings> settings, ILogger<WebhooksReceivedController> logger, IHooksRepository hooksRepository)
    {
        _settings = settings.Value;
        _logger = logger;
        _hooksRepository = hooksRepository;
    }

    /// <summary>
    /// 处理新收到的Webhook请求
    /// </summary>
    /// <param name="hook">webhook请求数据</param>
    /// <returns>处理结果</returns>
    [HttpPost]
    public async Task<IActionResult> NewWebhook(WebhookData hook)
    {
        // 从请求头中获取验证令牌
        var header = Request.Headers[HeaderNames.WebHookCheckHeader];
        var token = header.FirstOrDefault();

        // 记录接收到的webhook和验证信息
        _logger.LogInformation("Received hook with token {Token}. My token is {MyToken}. Token validation is set to {ValidateToken}",
            token, _settings.Token, _settings.ValidateToken);

        // 验证令牌：如果不需要验证或令牌匹配
        if (!_settings.ValidateToken || _settings.Token == token)
        {
            _logger.LogInformation("Received hook is going to be processed");

            // 创建新的WebHook记录
            var newHook = new WebHookReceived()
            {
                Data = hook.Payload,  // webhook的主要数据内容
                When = hook.When,     // webhook接收时间
                Token = token         // 验证令牌
            };

            // 保存到仓库
            await _hooksRepository.AddNew(newHook);

            _logger.LogInformation("Received hook was processed.");
            return Ok(newHook);  // 返回200状态码和处理的webhook数据
        }

        // 验证失败的情况
        _logger.LogInformation("Received hook is NOT processed - Bad Request returned.");
        return BadRequest();  // 返回400状态码
    }
}
