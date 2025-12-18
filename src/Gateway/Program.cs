using ServiceDefaults;
using Yarp.ReverseProxy.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddReverseProxy()
    .LoadFromMemory(
        new[]
        {
            new RouteConfig
            {
                RouteId = "procurement",
                ClusterId = "procurement",
                Match = new RouteMatch { Path = "/api/procurement/{**catch-all}" }
            },
            new RouteConfig
            {
                RouteId = "inventory",
                ClusterId = "inventory",
                Match = new RouteMatch { Path = "/api/inventory/{**catch-all}" }
            }
        },
        new[]
        {
            new ClusterConfig
            {
                ClusterId = "procurement",
                Destinations = new Dictionary<string, DestinationConfig>
                {
                    ["primary"] = new DestinationConfig { Address = "http://procurement-api" }
                }
            },
            new ClusterConfig
            {
                ClusterId = "inventory",
                Destinations = new Dictionary<string, DestinationConfig>
                {
                    ["primary"] = new DestinationConfig { Address = "http://inventory-api" }
                }
            }
        })
    .AddServiceDiscoveryDestinationResolver();

var app = builder.Build();

app.UseServiceDefaults();
app.MapReverseProxy();

app.MapGet("/", () => Results.Ok(new
{
    Message = "Gateway is running",
    Routes = new[] { "/api/procurement", "/api/inventory" }
}));

app.Run();
