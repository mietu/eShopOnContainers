namespace Microsoft.eShopOnContainers.Services.Ordering.SignalrHub;

/// <summary>
/// 通知中心类，处理客户端与服务器之间的实时通信
/// [Authorize] 特性确保只有经过身份验证的用户才能连接到此Hub
/// </summary>
[Authorize]
public class NotificationsHub : Hub
{
    /// <summary>
    /// 当客户端连接到Hub时调用的方法
    /// </summary>
    /// <returns>表示异步操作的任务</returns>
    public override async Task OnConnectedAsync()
    {
        // 将连接的客户端添加到以用户名命名的组中
        // Context.ConnectionId: 客户端的唯一连接ID
        // Context.User.Identity.Name: 连接用户的用户名
        await Groups.AddToGroupAsync(Context.ConnectionId, Context.User.Identity.Name);

        // 调用基类的OnConnectedAsync方法完成连接过程
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// 当客户端断开与Hub的连接时调用的方法
    /// </summary>
    /// <param name="ex">导致断开连接的异常（如果有）</param>
    /// <returns>表示异步操作的任务</returns>
    public override async Task OnDisconnectedAsync(Exception ex)
    {
        // 将断开连接的客户端从用户组中移除
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, Context.User.Identity.Name);

        // 调用基类的OnDisconnectedAsync方法完成断开连接过程
        await base.OnDisconnectedAsync(ex);
    }
}
