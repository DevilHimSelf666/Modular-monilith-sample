# Modular Monolith Sample

A .NET 9 modular monolith orchestrated with .NET Aspire. Procurement and Inventory bounded contexts are isolated by contracts, databases, and messaging (MassTransit + RabbitMQ with outbox/inbox patterns). Blazor Server provides a lightweight UI and Elsa workflow registration.

## Projects
- `src/AppHost`: Aspire orchestrator (SQL Server for Procurement/Inventory/Asset DBs, RabbitMQ, Gateway, Blazor, APIs).
- `src/ServiceDefaults`: Health/OTel/service discovery defaults used by all services.
- `src/Gateway`: YARP reverse proxy using Aspire service discovery to route to module APIs.
- `src/UI/Blazor.Server`: Blazor Server UI with Elsa registrations and simple screens for Procurement/Inventory.
- `src/Modules/*`: Domain/Application/Infrastructure/Contracts/Api per bounded context.
- `tests/*`: Unit and architecture tests (NetArchTest guardrails, idempotency verification).
- `docs/ARCHITECTURE_GUARDRAILS.md`: Non-negotiable boundary rules.
- `RUNBOOK.md`: Operational checklist.

## Running
```bash
dotnet restore
dotnet build
dotnet test
dotnet run --project src/AppHost/AppHost.csproj
```
Requires Aspire DCP CLI + dashboard binaries to orchestrate containers locally.

## Key endpoints
- Gateway (proxy): `http://localhost:5080`
- Procurement API: `GET /` and `POST /api/procurement/requests`
- Inventory API: `GET /` and `GET /api/inventory/reservations`
- Health for each service: `GET /health`
- Blazor UI: `http://localhost:5085` (proxied if configured)

## Messaging & persistence
- Procurement publishes `ProcurementCreatedIntegrationEvent` via MassTransit EF outbox (SQL Server, schema `proc`).
- Inventory consumes with an Inbox table (SQL Server schema `inv`) and records reservations idempotently.
- Retry/redelivery settings are applied with prefetch/concurrency tuning from configuration (`Messaging` section).

## Testing
- `tests/Procurement.Application.Tests`: command handler behavior + metadata.
- `tests/Inventory.Infrastructure.Tests`: Inbox/idempotency verification.
- `tests/ArchitectureTests`: Guardrails for purity and bounded contexts.
