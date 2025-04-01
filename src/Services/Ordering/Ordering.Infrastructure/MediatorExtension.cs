namespace Microsoft.eShopOnContainers.Services.Ordering.Infrastructure;

// 静态扩展方法类，用于处理领域事件的分发
static class MediatorExtension
{
    /// <summary>
    /// 扩展方法：异步分发领域事件
    /// </summary>
    /// <param name="mediator">消息中介者，用于发布领域事件</param>
    /// <param name="ctx">OrderingContext上下文，包含需要处理的实体</param>
    /// <returns>任务</returns>
    public static async Task DispatchDomainEventsAsync(this IMediator mediator, OrderingContext ctx)
    {
        // 从ChangeTracker中获取含有领域事件的实体记录
        // 条件：实体的DomainEvents属性不为空且其中存在事件
        var domainEntities = ctx.ChangeTracker
            .Entries<Entity>()
            .Where(x => x.Entity.DomainEvents != null && x.Entity.DomainEvents.Any());

        // 将所有领域事件收集到一个List中
        var domainEvents = domainEntities
            .SelectMany(x => x.Entity.DomainEvents)
            .ToList();

        // 清除所有实体中的领域事件，防止重复处理
        domainEntities.ToList()
            .ForEach(entity => entity.Entity.ClearDomainEvents());

        // 循环遍历每个领域事件，并通过mediator发布
        foreach (var domainEvent in domainEvents)
            await mediator.Publish(domainEvent);
    }
}
