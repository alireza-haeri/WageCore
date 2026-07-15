# Architecture Rules

WageCore uses **plain layered Clean Architecture** — explicitly **not** a modular monolith. There is one Core, one Application, one Infrastructure. Do not introduce module boundaries, module-per-feature folders at the solution level, or separate module assemblies. Feature separation happens *inside* Application via feature folders (see `cqrs-conventions.md`), not via separate projects.

## Layers and Dependency Direction

```
Web.Api  →  Infrastructure  →  Application  →  Core
  │                                              ↑
  └──────────────────────────────────────────────┘
              (Web.Api also references Core directly for DTOs)

Shared.Kernel  → referenced by Core, Application, Infrastructure, Web.Api
Shared.Web     → referenced by Web.Api (and Infrastructure if it needs web-context helpers)
```

Dependencies only ever point **inward**. Core never references Application, Infrastructure, or Web.Api.

### Core
- Domain entities and value objects
- DTOs shared across layers
- Abstractions (interfaces) that Infrastructure implements: `IPayrollRepository`, `IEmployeeRepository`, `ITenantProvider`, `IAuditLogger`, `IRuleEngine`, etc.
- Domain-level business rules that are true regardless of application flow (e.g. an entity's own invariants)
- **No dependency on EF Core, MediatR, ASP.NET Core, or any framework.** Core must compile as a plain .NET class library with no web/ORM references.

### Application
- UseCases: one folder per feature, each containing a Command or Query, its Handler, and its Validator (see `cqrs-conventions.md`)
- Orchestration logic: calling repositories/services through Core abstractions, composing the result
- MediatR pipeline behaviors (validation, logging, transaction wrapping) live here
- References Core only. Never references Infrastructure or Web.Api directly — Infrastructure is injected via interfaces defined in Core.

### Infrastructure
- EF Core `DbContext`, entity configurations, migrations
- Implementations of Core abstractions (`PayrollRepository : IPayrollRepository`)
- External integrations: PDF generation, background job scheduling (Hangfire), file storage
- Multi-tenancy plumbing: global query filters, tenant resolution
- References Core (and Application only if it needs to register handlers/behaviors during DI setup — prefer keeping this minimal)

### Shared.Kernel
- `Result` / `Result<T>` pattern
- Base entity classes (`Entity`, `AuditableEntity`, `ITenantScoped`)
- Generic extension methods with no framework dependency
- Referenced by every other project

### Shared.Web
- Cross-cutting ASP.NET Core concerns: exception-handling middleware, `ProblemDetails` mapping, common filters/attributes
- Referenced by Web.Api

### Web.Api
- Composition root: `Program.cs`, DI registration for all layers
- Minimal API endpoint definitions (see `api-conventions.md`)
- No business logic — endpoints should be thin, delegating everything to MediatR

## Where Does X Go? — Quick Decision Guide

| You're adding...                                  | It goes in...                                                            |
|---------------------------------------------------|--------------------------------------------------------------------------|
| A new domain entity or value object               | `Core/Domain`                                                            |
| A new interface that Infrastructure must implement | `Core/Abstractions`                                                      |
| A DTO shared between Application and Web.Api      | `Core/Contracts`                                                         |
| A new Command/Query + Handler + Validator         | `Application/Features/{FeatureName}/(Commands or Queries)/{UseCaseName}` |
| An EF Core entity migration       | `Infrastructure/Persistence/Migrations`                                  |
| An EF Core entity configuration      | `Infrastructure/Persistence/Configurations`                              |
| A repository implementation                       | `Infrastructure/Repositories`                                            |
| A service implementation                          | `Infrastructure/Services`                                                |
| A new Minimal API endpoint                        | `Web.Api` (grouped by feature)                                           |
| A background job                                  | `Infrastructure` (job class) + trigger registration in `Web.Api`         |

## When You're Unsure

If a task doesn't obviously fit the table above, default to the more restrictive layer (Core over Application, Application over Infrastructure) and ask before creating a new top-level folder or project.