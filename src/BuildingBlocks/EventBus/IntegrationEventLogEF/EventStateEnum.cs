namespace Microsoft.eShopOnContainers.BuildingBlocks.IntegrationEventLogEF;

// 定义集成事件的状态枚举，用于标识事件在发布过程中的不同状态
public enum EventStateEnum
{
    // 未发布：初始状态，事件尚未被发布
    NotPublished = 0,

    // 发布中：事件正在处理或者发布中
    InProgress = 1,

    // 已发布：事件已成功发布
    Published = 2,

    // 发布失败：事件的发布过程出现错误导致失败
    PublishedFailed = 3
}

