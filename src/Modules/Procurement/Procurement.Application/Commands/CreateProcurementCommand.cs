using MediatR;
using Microsoft.Extensions.Logging;
using Procurement.Application.Abstractions;
using Procurement.Contracts;
using Procurement.Domain;

namespace Procurement.Application.Commands;

public sealed record CreateProcurementCommand(string InquiryType, decimal Amount) : IRequest<ProcurementRequestDto>;

public sealed class CreateProcurementCommandHandler : IRequestHandler<CreateProcurementCommand, ProcurementRequestDto>
{
    private const int EventVersion = 1;
    private readonly IBudgetAvailabilityChecker _budgetAvailabilityChecker;
    private readonly IIntegrationEventPublisher _eventPublisher;
    private readonly ILogger<CreateProcurementCommandHandler> _logger;
    private readonly IProcurementRequestRepository _repository;
    private readonly IWorkflowStarter _workflowStarter;

    public CreateProcurementCommandHandler(
        IBudgetAvailabilityChecker budgetAvailabilityChecker,
        IIntegrationEventPublisher eventPublisher,
        ILogger<CreateProcurementCommandHandler> logger,
        IProcurementRequestRepository repository,
        IWorkflowStarter workflowStarter)
    {
        _budgetAvailabilityChecker = budgetAvailabilityChecker;
        _eventPublisher = eventPublisher;
        _logger = logger;
        _repository = repository;
        _workflowStarter = workflowStarter;
    }

    public async Task<ProcurementRequestDto> Handle(CreateProcurementCommand request, CancellationToken cancellationToken)
    {
        if (!await _budgetAvailabilityChecker.IsBudgetAvailableAsync(request.Amount, cancellationToken))
        {
            throw new InvalidOperationException($"Budget not available for amount {request.Amount}.");
        }

        var procurementRequest = new ProcurementRequest(request.InquiryType, request.Amount);
        var workflowInstanceId = await _workflowStarter.StartProcurementWorkflowAsync(procurementRequest, cancellationToken);
        procurementRequest.AssignWorkflow(workflowInstanceId);

        await _repository.AddAsync(procurementRequest, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        var messageId = Guid.NewGuid();
        var correlationId = procurementRequest.Id;
        var integrationEvent = new ProcurementCreatedIntegrationEvent(
            messageId,
            correlationId,
            procurementRequest.Id,
            procurementRequest.InquiryType,
            procurementRequest.Amount,
            procurementRequest.WorkflowInstanceId,
            DateTime.UtcNow,
            EventVersion);

        await _eventPublisher.PublishAsync(integrationEvent, messageId, correlationId, cancellationToken);

        _logger.LogInformation(
            "Created procurement request {RequestId} with workflow {WorkflowInstanceId}",
            procurementRequest.Id,
            procurementRequest.WorkflowInstanceId);

        return new ProcurementRequestDto(
            procurementRequest.Id,
            procurementRequest.InquiryType,
            procurementRequest.Amount,
            procurementRequest.WorkflowInstanceId);
    }
}
