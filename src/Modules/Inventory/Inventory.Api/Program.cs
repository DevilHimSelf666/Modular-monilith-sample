using Inventory.Application;
using Inventory.Contracts;
using Inventory.Infrastructure.Data;
using Inventory.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddInventoryApplication();
builder.Services.AddInventoryInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseServiceDefaults();

app.MapGet("/api/inventory/reservations", async (InventoryDbContext dbContext, CancellationToken cancellationToken) =>
{
    var reservations = await dbContext.ProcurementReservations
        .OrderByDescending(x => x.CreatedAtUtc)
        .Select(x => new ReservationDto(x.ProcurementRequestId, x.InquiryType, x.Amount, x.CreatedAtUtc))
        .ToListAsync(cancellationToken);

    return Results.Ok(reservations);
});

app.MapGet("/", () => Results.Ok(new { Message = "Inventory API" }));

app.Run();
