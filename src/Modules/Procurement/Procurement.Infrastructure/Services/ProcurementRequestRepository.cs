using Procurement.Application.Abstractions;
using Procurement.Domain;
using Procurement.Infrastructure.Data;

namespace Procurement.Infrastructure.Services;

public class ProcurementRequestRepository : IProcurementRequestRepository
{
    private readonly ProcurementDbContext _dbContext;

    public ProcurementRequestRepository(ProcurementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(ProcurementRequest request, CancellationToken cancellationToken = default)
    {
        await _dbContext.ProcurementRequests.AddAsync(request, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
