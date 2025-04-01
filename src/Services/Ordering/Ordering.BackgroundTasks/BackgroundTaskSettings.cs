namespace Ordering.BackgroundTasks
{
    /// <summary>
    /// 后台任务设置类，封装了后台任务运行时所需的配置参数。
    /// </summary>
    public class BackgroundTaskSettings
    {
        /// <summary>
        /// 获取或设置数据库连接字符串。
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// 获取或设置事件总线连接字符串，用于与事件总线通信。
        /// </summary>
        public string EventBusConnection { get; set; }

        /// <summary>
        /// 获取或设置宽限期时间，通常用于延迟处理任务的时间（单位：秒）。
        /// </summary>
        public int GracePeriodTime { get; set; }

        /// <summary>
        /// 获取或设置检查更新的时间间隔，控制任务更新频率（单位：秒）。
        /// </summary>
        public int CheckUpdateTime { get; set; }

        /// <summary>
        /// 获取或设置订阅客户端名称，用于标识客户端进行事件订阅。
        /// </summary>
        public string SubscriptionClientName { get; set; }
    }
}
