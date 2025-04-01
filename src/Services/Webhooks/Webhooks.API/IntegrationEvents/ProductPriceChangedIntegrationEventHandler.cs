namespace Webhooks.API.IntegrationEvents;

/// <summary>
/// //定义事件处理
/// </summary>
public class ProductPriceChangedIntegrationEventHandler : IIntegrationEventHandler<ProductPriceChangedIntegrationEvent>
{
    public async Task Handle(ProductPriceChangedIntegrationEvent @event)
    {
        int i = 0;
    }
}
