using Dapper;
using Microsoft.eShopOnContainers.BuildingBlocks.EventBus.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ordering.BackgroundTasks.Events;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace Ordering.BackgroundTasks.Services
{
    /// <summary>
    /// 该服务负责管理宽限期内确认的订单，并对外发布相应的集成事件
    /// </summary>
    public class GracePeriodManagerService : BackgroundService
    {
        // 日志记录实例
        private readonly ILogger<GracePeriodManagerService> _logger;
        // 后台任务配置，包括数据库连接字符串及时间设定等
        private readonly BackgroundTaskSettings _settings;
        // 事件总线，用于发布订单处理相关的集成事件
        private readonly IEventBus _eventBus;

        /// <summary>
        /// 构造函数，初始化服务依赖项
        /// </summary>
        /// <param name="settings">包含后台任务配置的选项</param>
        /// <param name="eventBus">事件总线实例</param>
        /// <param name="logger">日志记录实例</param>
        public GracePeriodManagerService(IOptions<BackgroundTaskSettings> settings, IEventBus eventBus, ILogger<GracePeriodManagerService> logger)
        {
            // 校验参数是否为null，并初始化私有字段
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// 覆写ExecuteAsync方法，该方法在服务启动时由宿主机调用，
        /// 进入一个循环不断检查宽限期内确认的订单，并发布它们的集成事件。
        /// </summary>
        /// <param name="stoppingToken">用于处理取消请求的令牌</param>
        /// <returns>异步任务</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogDebug("GracePeriodManagerService is starting.");

            // 注册取消令牌回调，当任务取消时记录日志
            stoppingToken.Register(() => _logger.LogDebug("#1 GracePeriodManagerService background task is stopping."));

            // 循环检查订单，直到收到取消请求
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogDebug("GracePeriodManagerService background task is doing background work.");

                // 检查宽限期内确认的订单，并发布相应事件
                CheckConfirmedGracePeriodOrders();

                // 延迟一段时间，时间间隔从配置中获取
                await Task.Delay(_settings.CheckUpdateTime, stoppingToken);
            }

            _logger.LogDebug("GracePeriodManagerService background task is stopping.");
        }

        /// <summary>
        /// 检 查 宽限期内确认的订单，并对每个匹配的订单发布一个集成事件
        /// </summary>
        private void CheckConfirmedGracePeriodOrders()
        {
            _logger.LogDebug("Checking confirmed grace period orders");

            // 获取满足宽限期条件的订单ID集合
            var orderIds = GetConfirmedGracePeriodOrders();

            // 对每个订单发布一个宽限期确认事件
            foreach (var orderId in orderIds)
            {
                // 创建一个新的宽限期确认集成事件
                var confirmGracePeriodEvent = new GracePeriodConfirmedIntegrationEvent(orderId);

                // 记录可以发布的事件信息，并打印日志
                _logger.LogInformation("----- Publishing integration event: {IntegrationEventId} from {AppName} - ({@IntegrationEvent})",
                    confirmGracePeriodEvent.Id, Program.AppName, confirmGracePeriodEvent);

                // 通过事件总线发布该事件
                _eventBus.Publish(confirmGracePeriodEvent);
            }
        }

        /// <summary>
        /// 从数据库中查询满足宽限期条件的订单ID集合
        /// </summary>
        /// <returns>满足条件的订单ID集合</returns>
        private IEnumerable<int> GetConfirmedGracePeriodOrders()
        {
            // 默认返回空的订单列表
            IEnumerable<int> orderIds = new List<int>();

            // 利用Dapper进行数据库操作，创建SQL连接
            using var conn = new SqlConnection(_settings.ConnectionString);
            try
            {
                conn.Open();

                // 查询订单：订单时间与当前时间相差超过指定宽限期且订单状态为1（已确认）
                orderIds = conn.Query<int>(
                    @"SELECT Id FROM [ordering].[orders] 
                          WHERE DATEDIFF(minute, [OrderDate], GETDATE()) >= @GracePeriodTime
                            AND [OrderStatusId] = 1",
                    new { _settings.GracePeriodTime });
            }
            catch (SqlException exception)
            {
                // 数据库访问失败时记录严重错误日志
                _logger.LogCritical(exception, "FATAL ERROR: Database connections could not be opened: {Message}", exception.Message);
            }

            return orderIds;
        }
    }
}
