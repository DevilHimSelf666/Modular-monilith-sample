# Codex Agent Instructions — Mellat Bank Comprehensive Support System (Modular Monolith)

## Role
You are a senior .NET Solutions Architect. Optimize for correctness, Clean Architecture, and enterprise DDD patterns.

## Hard Constraints
- Target framework: .NET 9
- Architecture: Modular Monolith, Clean Architecture per module
- Physical DB isolation: separate SQL Server database per module (ProcurementDb, InventoryDb, AssetDb)
- App orchestration: .NET Aspire (AppHost + ServiceDefaults)
- Async messaging: MassTransit + RabbitMQ
- In-process sync: MediatR
- Workflow: Elsa Workflows v3 integrated (embedded in Blazor; activities implemented in Application layer)
- API Gateway: YARP reverse proxy
- UI: Blazor Server (Interactive Server)
- Code style: implicit usings, file-scoped namespaces, nullable enabled
- Must run: `dotnet run --project src/AppHost/AppHost.csproj`

## Operating Principles
- SOLID + DRY, favor small cohesive projects and explicit boundaries.
- Keep cross-module dependencies via `Contracts` only (DTOs + integration events).
- Enforce per-module database isolation (no shared DbContext).
- Use Outbox Pattern for publishing integration events.
- Favor minimal, production-oriented defaults (health checks, OTel, structured logging).

## Repository Layout (create if missing)
- src/
  - AppHost/
  - ServiceDefaults/
  - Gateway/
  - UI/Blazor.Server/
  - Modules/
    - Procurement/
      - Procurement.Domain/
      - Procurement.Application/
      - Procurement.Infrastructure/
      - Procurement.Contracts/
      - Procurement.Api/
    - Inventory/
      - Inventory.Domain/
      - Inventory.Application/
      - Inventory.Infrastructure/
      - Inventory.Contracts/
      - Inventory.Api/
  - BuildingBlocks/ (only if needed for shared abstractions; keep minimal)
- Directory.Packages.props at repo root

## Implementation Rules
1. **AppHost (Aspire)**
   - Configure a SQL Server resource.
   - Define separate databases: ProcurementDb, InventoryDb, AssetDb.
   - Register RabbitMQ resource.
   - Wire up Procurement.Api, Inventory.Api, Gateway, and Blazor.Server with correct references and endpoints.

2. **ServiceDefaults**
   - Add OpenTelemetry tracing/metrics/logging, health checks, and service discovery defaults suitable for Aspire.

3. **Procurement Module**
   - Domain:
     - Aggregate root: `ProcurementRequest` with `InquiryType`, `Amount`, `WorkflowInstanceId`.
   - Application:
     - `CreateProcurementCommand` + handler (MediatR).
     - Custom Elsa Activity: `BudgetAvailabilityActivity` that checks budget via `IBudgetAvailabilityChecker`.
   - Infrastructure:
     - `ProcurementDbContext` using schema `proc`.
     - MassTransit outbox configured; publish `ProcurementCreatedIntegrationEvent`.
   - Contracts:
     - DTOs + `ProcurementCreatedIntegrationEvent` record.

4. **Inventory Module**
   - Domain:
     - Entities: `Warehouse`, `StockItem` (model basic fields + relationships).
   - Application:
     - Minimal commands/queries as needed for demo (keep lean).
   - Infrastructure:
     - `InventoryDbContext` with schema `inv` (or similar).
     - MassTransit consumer for `ProcurementCreatedIntegrationEvent` that reacts deterministically (e.g., reserve stock or create a tracking record).
   - Contracts:
     - Inventory DTOs if required; event consumption uses Procurement.Contracts reference only.

5. **Gateway (YARP)**
   - Provide a working YARP config routing:
     - `/api/procurement/{**catch-all}` → Procurement.Api
     - `/api/inventory/{**catch-all}` → Inventory.Api

6. **Blazor.Server**
   - Register Elsa Dashboard UI and required Elsa services.
   - Show workflow registration and how to start/track workflow instances (minimal example is fine).
   - Ensure custom Activities from Application layer are registered.

## Acceptance Criteria (must verify)
- Solution builds without errors.
- Running AppHost starts: Gateway, Procurement.Api, Inventory.Api, Blazor.Server, RabbitMQ, and SQL Server resource bindings.
- Procurement API endpoint can create a procurement request and triggers an integration event via outbox.
- Inventory module successfully consumes `ProcurementCreatedIntegrationEvent` (log evidence + minimal persistence or in-memory effect).
- Elsa dashboard is reachable from Blazor Server app.
- Health checks available for APIs and UI.
- Central Package Management present and used (no scattered package versions).

## Testing Guidance (lightweight)
- Add at least one unit test for Procurement.Application command handler behavior.
- Add at least one consumer test or integration-style test for Inventory consumer (can be simplified; prioritize compile/run reliability).

## Deliverables
- All new projects, solution file updates, configs, and minimal docs (README) describing how to run.
