using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

var builder = DistributedApplication.CreateBuilder(args);

var sqlPassword = builder.AddParameter("sql-password", secret: true);
var rabbitUser = builder.AddParameter("rabbit-username", value: "app", publishValueAsDefault: true, secret: false);
var rabbitPassword = builder.AddParameter("rabbit-password", secret: true);

var sqlServer = builder.AddSqlServer("sql").WithPassword(sqlPassword);
var procurementDb = sqlServer.AddDatabase("ProcurementDb");
var inventoryDb = sqlServer.AddDatabase("InventoryDb");
var assetDb = sqlServer.AddDatabase("AssetDb");

var rabbit = builder.AddRabbitMQ("rabbitmq", userName: rabbitUser, password: rabbitPassword);

var procurementApi = builder.AddProject("procurement-api", "../Modules/Procurement/Procurement.Api/Procurement.Api.csproj")
    .WithReference(procurementDb)
    .WithReference(rabbit)
    .WithEnvironment("ConnectionStrings__ProcurementDb", procurementDb.Resource.ConnectionStringExpression)
    .WithEnvironment("ConnectionStrings__rabbitmq", rabbit.Resource.ConnectionStringExpression)
    .WithEnvironment("Budget__MaxAmount", "1000000");

var inventoryApi = builder.AddProject("inventory-api", "../Modules/Inventory/Inventory.Api/Inventory.Api.csproj")
    .WithReference(inventoryDb)
    .WithReference(rabbit)
    .WithEnvironment("ConnectionStrings__InventoryDb", inventoryDb.Resource.ConnectionStringExpression)
    .WithEnvironment("ConnectionStrings__rabbitmq", rabbit.Resource.ConnectionStringExpression);

var gateway = builder.AddProject("gateway", "../Gateway/Gateway.csproj")
    .WithReference(procurementApi)
    .WithReference(inventoryApi);

builder.AddProject("blazor-server", "../UI/Blazor.Server/Blazor.Server.csproj")
    .WithReference(procurementApi)
    .WithReference(inventoryApi)
    .WithReference(gateway)
    .WithEnvironment("ConnectionStrings__AssetDb", assetDb.Resource.ConnectionStringExpression)
    .WithEnvironment("ConnectionStrings__ProcurementDb", procurementDb.Resource.ConnectionStringExpression);

builder.Build().Run();
