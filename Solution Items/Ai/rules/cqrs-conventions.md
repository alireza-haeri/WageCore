# CQRS / UseCase Conventions

Application logic is organized as **UseCases** using MediatR, following a feature-folder pattern. Every UseCase is Command or Query, never both.

## Folder Structure

```
Application/
└── Features/
    └── {FeatureName}/                     e.g. Employees, Payroll, Attendance
        └── Command or Queries/                 
            └── {UseCaseName}/                 e.g. CreateEmployee, CalculateMonthlyPayroll
                ├── {UseCaseName}Command.cs     (or ...Query.cs) and response
                ├── {UseCaseName}Handler.cs
                ├── {UseCaseName}Validator.cs
```

## Naming

- Commands: verb + noun, imperative — `CreateEmployeeCommand`, `CalculateMonthlyPayrollCommand`, `RecordAbsenceCommand`.
- Queries: `Get`/`List` + noun — `GetEmployeeByIdQuery`, `ListPayslipsForMonthQuery`.
- Handlers: `{CommandOrQueryName}Handler` — `CreateEmployeeCommandHandler`.
- Validators: `{CommandOrQueryName}Validator` — `CreateEmployeeCommandValidator`.

## Command/Query Shape

- Commands and Queries are `record`s implementing `IRequest<Result<TResponse>>` (or `IRequest<Result>` if there's no meaningful return payload).
- They carry only the data needed for that operation — not entire entities. Map from/to Core DTOs or entities inside the Handler, not inside the Command itself.
- `TenantId` is **not** a field on the Command/Query — it's resolved inside the Handler via `ITenantProvider`, never passed in from the client.

## Handlers

- One Handler per Command/Query. Handlers are thin: validate preconditions not already covered by the Validator, call Core abstractions (repositories/services), map the result, return `Result<T>`.
- No business rule that belongs in the domain/rule engine should live inline in a Handler — call out to `IRuleEngine` or the relevant domain service instead.
- Handlers do not catch exceptions to convert them into `Result` failures for expected cases — expected failures should already surface as `Result` failures from the layers they call. Handlers only deal with genuinely unexpected exceptions by letting them propagate to a pipeline behavior / global handler.

## Validators

- Use FluentValidation. Every Command gets a Validator, even if it only validates "not null / not empty" — this keeps the pattern consistent and lets a MediatR validation pipeline behavior handle all of them uniformly.
- Validation here means **input shape/format validation** (required fields, ranges, formats) — not business rules that depend on database state (e.g. "employee must not already have a payslip for this month" is a Handler-level business check, not a Validator concern), unless the project's pipeline behavior explicitly supports async DB-backed validation.

## Pipeline Behaviors

- Validation behavior: runs all registered Validators before the Handler executes, short-circuits with a `Result` failure on validation errors.
- Logging behavior: logs Command/Query name and (sanitized) key parameters, and outcome.
- Keep pipeline behaviors in `Application/Common/Behaviors` and register them in DI in a fixed, deliberate order (validation before logging, or vice versa — pick one and stay consistent).

## Queries vs Commands

- Queries never mutate state. If a "query" needs to write something (e.g. caching, view tracking), reconsider — that's usually a sign it should be a Command or handled elsewhere.
- Queries can bypass the full repository abstraction and use a lighter read path (e.g. projection directly to a DTO) when performance matters, as long as tenant filtering is still respected.