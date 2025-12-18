# Runbook

## Running the distributed app
1. Restore/build/tests:
   ```bash
   dotnet restore
   dotnet build
   dotnet test
   ```
2. Start the Aspire orchestrator:
   ```bash
   dotnet run --project src/AppHost/AppHost.csproj
   ```
   Requires Aspire DCP CLI + dashboard binaries. SQL Server and RabbitMQ containers are orchestrated automatically; passwords are provided via AppHost parameters.

## Health endpoints
- Gateway: `GET /health`
- Procurement API: `GET /health`
- Inventory API: `GET /health`
- Blazor UI: `GET /health`

## Aspire dashboard / telemetry
If the Aspire dashboard is available, it is served by the orchestrator. Open the dashboard URL emitted by `dotnet run --project src/AppHost/AppHost.csproj` to inspect resources, health, and logs. OpenTelemetry exporters are enabled when `OTEL_EXPORTER_OTLP_ENDPOINT` is configured.

## Verifying messaging consistency
- **Outbox (publisher)**: Procurement uses MassTransit EF outbox. Check the `__Audit`/outbox tables in `ProcurementDb` (schema `proc`) after posting to `/api/procurement/requests`; an outbox row is created and dispatched by MassTransit.
- **Inbox (consumer)**: Inventory persists processed message IDs in `inv.InboxMessages`. The consumer records a single `ProcurementReservation` per `MessageId`.
- **Expected logs**:
  - Outbox insert & publish: `Created procurement request ...` (Procurement API) and MassTransit outbox dispatch logs.
  - Consumer receive & dedupe: `Recorded procurement reservation ...` for the first delivery; `Duplicate message ... ignored` for duplicates.

## Manual validation steps
1. POST to Procurement API via Gateway:
   ```bash
   curl -X POST http://localhost:5080/api/procurement/requests \
     -H "Content-Type: application/json" \
     -d '{"inquiryType":"Laptops","amount":5000}'
   ```
2. Inspect `ProcurementDb` for outbox entries and the `proc.ProcurementRequests` row.
3. Query Inventory reservations:
   ```bash
   curl http://localhost:5080/api/inventory/reservations
   ```
4. Re-run the same POST with the same `MessageId` (or publish duplicate via tests) and confirm only one reservation row exists while `inv.InboxMessages` contains the message ID.
