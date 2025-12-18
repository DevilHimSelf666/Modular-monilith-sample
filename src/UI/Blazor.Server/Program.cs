using Blazor.Server.Components;
using Elsa.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.ServiceDiscovery;
using Elsa.EntityFrameworkCore.Modules.Management;
using Elsa.EntityFrameworkCore.Modules.Runtime;
using ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddHttpClient("Gateway", client => client.BaseAddress = new Uri("http://gateway"))
    .AddServiceDiscovery()
    .AddStandardResilienceHandler();

var workflowConnection = builder.Configuration.GetConnectionString("AssetDb")
                        ?? builder.Configuration.GetConnectionString("ProcurementDb")
                        ?? "Data Source=workflows.db";

builder.Services.AddElsa(elsa => elsa
    .UseWorkflowManagement(management => management.UseEntityFrameworkCore(options =>
    {
        options.DbContextOptionsBuilder = (_, db) => db.UseSqlServer(workflowConnection);
        options.UseContextPooling = true;
    }))
    .UseWorkflowRuntime(runtime => runtime.UseEntityFrameworkCore(options =>
    {
        options.DbContextOptionsBuilder = (_, db) => db.UseSqlServer(workflowConnection);
        options.UseContextPooling = true;
    }))
    .AddActivitiesFrom<Program>()
    .AddWorkflowsFrom<Program>());

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

app.UseServiceDefaults();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapGet("/elsa", () => Results.Ok(new
{
    Message = "Elsa workflow endpoints registered via UseWorkflowManagement/UseWorkflowRuntime",
    ApiRoot = "/workflows"
}));

app.Run();
