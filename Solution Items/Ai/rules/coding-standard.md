# Coding Standards

## General
- Language: C# (latest version supported by .NET 10), file-scoped namespaces everywhere.
- Nullable reference types are **enabled** — treat every warning as something to fix, not suppress.
- Prefer `record`/`record struct` for DTOs and value objects; use `class` for entities and services.
- Use primary constructors where they reduce boilerplate (services, handlers with injected dependencies).
- One public type per file. File name matches the type name.

## Naming
- `PascalCase` for types, methods, properties, public fields, constants.
- `camelCase` for locals and parameters.
- `_camelCase` for private fields.
- Interfaces prefixed with `I` (`IPayrollRepository`).
- Async methods suffixed with `Async` (`CalculatePayrollAsync`).
- Boolean properties/methods read as a question (`IsActive`, `HasOverlap`, `CanApprove`).

## Async
- Every I/O-bound method is `async` and returns `Task`/`Task<T>`. No `async void` except top-level event handlers, and even that should be avoided.
- Always accept and forward a `CancellationToken` in Application/Infrastructure methods that do I/O.
- Never block on async code with `.Result` or `.Wait()`.

## Errors and the Result Pattern
- Expected/business failures (validation errors, "employee not found", "tenant mismatch") return `Result` / `Result<T>` from `Shared.Kernel` — they are **not** exceptions.
- Exceptions are reserved for truly unexpected failures (infrastructure errors, bugs). Don't use exceptions for control flow.
- Never swallow an exception silently. If you catch it, log it or rethrow it.

## Money and Numbers
- All monetary values are `decimal`. Never `double` or `float` for anything related to salary, tax, insurance, or currency.
- Rounding rules are explicit and centralized (see `domain-rules.md`) — never round ad hoc inline in a handler.
- Magic numbers related to payroll rules (percentages, thresholds) are forbidden — they come from the rule engine/configuration, not literals in code.

## Dependency Injection
- Constructor injection only. No service locator pattern, no static access to `IServiceProvider`.
- Interfaces live in Core; implementations live in Infrastructure; registration happens in Web.Api's `Program.cs` (or extension methods called from it, e.g. `AddInfrastructure()`, `AddApplication()`).

## Comments and Documentation
- Code should be self-explanatory through naming; use comments to explain **why**, not **what**.
- Any non-obvious payroll rule (e.g. a specific rounding or proration rule from Iranian labor law) gets a comment explaining the rule, ideally with a reference to the legal source or configuration key it maps to.

## Formatting
- Follow standard `dotnet format` / `.editorconfig` rules if present in the repo; if not, default to the built-in Roslyn/VS style (4-space indents, braces on new lines for types/methods).