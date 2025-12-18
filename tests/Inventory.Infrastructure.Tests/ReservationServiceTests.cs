using Inventory.Infrastructure.Data;
using Inventory.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Procurement.Contracts;
using Xunit;
using System.Linq;

namespace Inventory.Infrastructure.Tests;

public class ReservationServiceTests
{
    [Fact]
    public async Task Duplicate_message_id_is_ignored()
    {
        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new InventoryDbContext(options);
        var service = new ReservationService(dbContext);

        var messageId = Guid.NewGuid();
        var integrationEvent = new ProcurementCreatedIntegrationEvent(messageId, Guid.NewGuid(), Guid.NewGuid(), "IT", 1000, "wf", DateTime.UtcNow, 1);

        var first = await service.RecordAsync(messageId, integrationEvent, CancellationToken.None);
        var second = await service.RecordAsync(messageId, integrationEvent, CancellationToken.None);

        Assert.True(first);
        Assert.False(second);
        Assert.Equal(1, dbContext.ProcurementReservations.Count());
        Assert.Equal(1, dbContext.InboxMessages.Count());
    }
}
