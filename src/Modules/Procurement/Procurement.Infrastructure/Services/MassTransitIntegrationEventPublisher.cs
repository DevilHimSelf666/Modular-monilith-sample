using MassTransit;
using Procurement.Application.Abstractions;

namespace Procurement.Infrastructure.Services;

public class MassTransitIntegrationEventPublisher : IIntegrationEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;

    public MassTransitIntegrationEventPublisher(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public Task PublishAsync<T>(T message, Guid messageId, Guid? correlationId = null, CancellationToken cancellationToken = default) where T : class
    {
        return _publishEndpoint.Publish(message, publishContext =>
        {
            publishContext.MessageId = messageId;
            if (correlationId.HasValue)
            {
                publishContext.CorrelationId = correlationId;
            }
        }, cancellationToken);
    }
}
