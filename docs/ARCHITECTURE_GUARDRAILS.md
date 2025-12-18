# Architecture Guardrails

These rules are mandatory for the modular monolith.

## Boundaries
- No cross-module references except through `*.Contracts` projects.
- No module may reference another module’s `Infrastructure` project.
- Each module owns its own persistence:
  - Separate SQL Server database per module (physical isolation).
  - Separate migrations per module; no shared migrations.

## Layering / Clean Architecture
- Domain projects are pure: **no** references to EF Core, MassTransit, Elsa, ASP.NET, or other web/framework packages.
- Application projects orchestrate use cases; Infrastructure implements adapters and providers.
- Cross-module communication uses integration events defined in Contracts—never direct calls.

## Messaging in a Modular Monolith
- Cross-module communication uses integration events only.
- Publishers must use the Transactional Outbox pattern for consistency.
- Consumers must be idempotent (Inbox/deduplication keyed by `MessageId`).
