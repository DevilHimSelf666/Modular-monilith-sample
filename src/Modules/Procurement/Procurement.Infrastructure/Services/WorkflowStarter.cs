using Microsoft.Extensions.Logging;
using Procurement.Application.Abstractions;
using Procurement.Domain;

namespace Procurement.Infrastructure.Services;

public class WorkflowStarter : IWorkflowStarter
{
    private readonly ILogger<WorkflowStarter> _logger;

    public WorkflowStarter(ILogger<WorkflowStarter> logger)
    {
        _logger = logger;
    }

    public Task<string> StartProcurementWorkflowAsync(ProcurementRequest request, CancellationToken cancellationToken = default)
    {
        var workflowInstanceId = Guid.NewGuid().ToString();
        _logger.LogInformation("Starting workflow {WorkflowInstanceId} for procurement {RequestId}", workflowInstanceId, request.Id);
        return Task.FromResult(workflowInstanceId);
    }
}
