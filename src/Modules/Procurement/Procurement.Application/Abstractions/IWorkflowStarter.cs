using Procurement.Domain;

namespace Procurement.Application.Abstractions;

public interface IWorkflowStarter
{
    Task<string> StartProcurementWorkflowAsync(ProcurementRequest request, CancellationToken cancellationToken = default);
}
