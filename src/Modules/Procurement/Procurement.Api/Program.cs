using MediatR;
using Procurement.Application;
using Procurement.Application.Commands;
using Procurement.Infrastructure.Extensions;
using ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddProcurementApplication();
builder.Services.AddProcurementInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseServiceDefaults();

app.MapPost("/api/procurement/requests", async (CreateProcurementCommand command, IMediator mediator, CancellationToken cancellationToken) =>
{
    var result = await mediator.Send(command, cancellationToken);
    return Results.Created($"/api/procurement/requests/{result.Id}", result);
});

app.MapGet("/", () => Results.Ok(new { Message = "Procurement API" }));

app.Run();
