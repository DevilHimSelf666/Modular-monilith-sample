using Inventory.Domain.Entities;
using Inventory.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Procurement.Contracts;

namespace Inventory.Infrastructure.Services;

public interface IReservationService
{
    Task<bool> RecordAsync(Guid messageId, ProcurementCreatedIntegrationEvent @event, CancellationToken cancellationToken);
}

public class ReservationService : IReservationService
{
    private readonly InventoryDbContext _dbContext;

    public ReservationService(InventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> RecordAsync(Guid messageId, ProcurementCreatedIntegrationEvent @event, CancellationToken cancellationToken)
    {
        if (await _dbContext.InboxMessages.AnyAsync(x => x.MessageId == messageId, cancellationToken))
        {
            return false;
        }

        _dbContext.InboxMessages.Add(new InboxMessage(messageId));

        var warehouse = await _dbContext.Warehouses.Include(x => x.StockItems).FirstOrDefaultAsync(cancellationToken)
                        ?? new Warehouse("Main", "Default");

        if (_dbContext.Entry(warehouse).State == EntityState.Detached)
        {
            _dbContext.Warehouses.Add(warehouse);
        }

        var reservation = new ProcurementReservation(messageId, @event.ProcurementRequestId, @event.InquiryType, @event.Amount);
        _dbContext.ProcurementReservations.Add(reservation);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
