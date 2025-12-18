namespace Procurement.Application.Abstractions;

public interface IIntegrationEventPublisher
{
    Task PublishAsync<T>(T message, Guid messageId, Guid? correlationId = null, CancellationToken cancellationToken = default) where T : class;
}
