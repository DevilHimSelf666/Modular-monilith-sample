using Procurement.Domain;

namespace Procurement.Application.Abstractions;

public interface IProcurementRequestRepository
{
    Task AddAsync(ProcurementRequest request, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
