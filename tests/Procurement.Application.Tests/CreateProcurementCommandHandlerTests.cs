using System.Collections.Concurrent;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using Procurement.Application.Abstractions;
using Procurement.Application.Commands;
using Procurement.Contracts;
using Procurement.Domain;
using Xunit;

namespace Procurement.Application.Tests;

public class CreateProcurementCommandHandlerTests
{
    [Fact]
    public async Task Creates_request_with_metadata_and_publishes_event()
    {
        var repository = new InMemoryRepository();
        var budgetChecker = new PermissiveBudgetChecker();
        var workflowStarter = new FixedWorkflowStarter("wf-123");
        var publisher = new RecordingPublisher();
        var handler = new CreateProcurementCommandHandler(budgetChecker, publisher, NullLogger<CreateProcurementCommandHandler>.Instance, repository, workflowStarter);

        var result = await handler.Handle(new CreateProcurementCommand("IT", 1000), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("wf-123", result.WorkflowInstanceId);
        Assert.Single(repository.Items);
        Assert.Single(publisher.Messages);

        var message = publisher.Messages.Single();
        Assert.NotEqual(Guid.Empty, message.Event.MessageId);
        Assert.Equal(message.Event.MessageId, message.ContextMessageId);
        Assert.Equal(result.Id, message.Event.ProcurementRequestId);
        Assert.True(message.Event.OccurredAtUtc <= DateTime.UtcNow);
        Assert.Equal(1, message.Event.Version);
    }

    private sealed record Published(ProcurementCreatedIntegrationEvent Event, Guid? ContextMessageId);

    private sealed class RecordingPublisher : IIntegrationEventPublisher
    {
        public List<Published> Messages { get; } = new();

        public Task PublishAsync<T>(T message, Guid messageId, Guid? correlationId = null, CancellationToken cancellationToken = default) where T : class
        {
            var integrationEvent = Assert.IsType<ProcurementCreatedIntegrationEvent>(message);
            Messages.Add(new Published(integrationEvent, messageId));
            return Task.CompletedTask;
        }
    }

    private sealed class InMemoryRepository : IProcurementRequestRepository
    {
        public ConcurrentBag<ProcurementRequest> Items { get; } = new();

        public Task AddAsync(ProcurementRequest request, CancellationToken cancellationToken = default)
        {
            Items.Add(request);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class PermissiveBudgetChecker : IBudgetAvailabilityChecker
    {
        public Task<bool> IsBudgetAvailableAsync(decimal amount, CancellationToken cancellationToken = default) => Task.FromResult(true);
    }

    private sealed class FixedWorkflowStarter : IWorkflowStarter
    {
        private readonly string _workflowInstanceId;

        public FixedWorkflowStarter(string workflowInstanceId)
        {
            _workflowInstanceId = workflowInstanceId;
        }

        public Task<string> StartProcurementWorkflowAsync(ProcurementRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_workflowInstanceId);
        }
    }
}
