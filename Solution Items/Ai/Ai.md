# WageCore — AI Assistant Rules

> This is the entry point for any AI tool (Claude, Cursor, Copilot, or any model) working on this codebase.
> Read this file fully on every task. Only open a file under `rules/` when the task actually touches that topic — don't preload everything.

## 1. What This Project Is

WageCore is a payroll calculation system for small businesses (restaurants, shops, service offices) — a lightweight alternative to heavy Iranian accounting suites (Holo, Sepidar). It does one thing well: **calculate payroll correctly and generate payslips**.

Core capabilities:
- Employee management
- Monthly attendance / absence / overtime / leave entry
- Automated payslip calculation with PDF output
- A **configurable rule engine** so yearly labor-law changes (insurance %, tax, severance, bonuses) don't require code changes or redeploys
- **Multi-tenancy** — each business is a fully isolated tenant
- **Audit log** — every change to salary or personnel data is recorded with timestamp + reason
- **Event-driven** processing — attendance events trigger downstream payroll recalculation
- **Scheduled background jobs** — end-of-month payslip generation runs automatically

## 2. Who's Working On This

Solo developer, no team. There is no PR-review process and no need for team-coordination overhead. Optimize for:
- Clarity a future-you can pick back up after months away
- Correctness of payroll math above all — money bugs are the worst class of bug here
- Small, reviewable, well-named commits (see `rules/git-workflow.md`)

## 3. Tech Stack

| Layer | Technology                                               |
|---|----------------------------------------------------------|
| Backend | ASP.NET Core 10, Controllers                             |
| Architecture | Clean Architecture (layered, **not** a modular monolith) |
| CQRS | MediatR                                                  |
| Validation | FluentValidation                                         |
| Database | SQL Server, EF Core                                      |
| Background jobs | Hangfire / `BackgroundService`                           |
| Auth | JWT, ASP.NET Core Identity                               |
| Frontend | Blazor WebAssembly                                       |

## 4. Solution Structure

```
WageCore/
├── Solution Items/
│   └── Ai/                    ← you are here
│       ├── Ai.md
│       └── rules/
├── src/
│   ├── Core/                  ← Domain models, DTOs, Abstractions (interfaces). No dependencies on anything else.
│   ├── Application/           ← UseCases: Command/Query + Handler + Validator (CQRS via MediatR). Depends on Core only.
│   ├── Infrastructure/        ← EF Core, Persistence, implementations of Core abstractions. Depends on Core (+ Application if needed for DI wiring).
│   ├── Shareds/
│   │   ├── Shared.Kernel/     ← Result pattern, base entities, common extensions. No project dependencies.
│   │   └── Shared.Web/        ← Cross-cutting web concerns (middleware, filters, web extensions).
│   └── Web.Api/                ← Composition root: Minimal API endpoints, DI wiring, Program.cs
└── WageCore.slnx
```

Full details on layering and dependency direction: `rules/architecture.md`.

## 5. Non-Negotiable Principles

1. **Money is `decimal`, never `double`/`float`.** No exceptions.
2. **Every domain rule that can change year-to-year (tax %, insurance %, overtime multiplier, etc.) belongs in the rule engine / configuration — never hard-coded.**
3. **Every query and write must be tenant-scoped.** A cross-tenant data leak is a critical bug, not a minor one.
4. **Every mutation to salary or personnel data must produce an audit log entry** (who/what/when/why).
5. **Use the Result pattern for expected failures** (validation, business-rule violations). Reserve exceptions for truly exceptional/unexpected situations.
6. Prefer explicit, boring code over clever abstractions. This is a solo project — optimize for re-readability, not for showing off patterns.

## 6. Reference Files — Read Only When Relevant

| File | Read this when the task involves... |
|---|---|
| `rules/architecture.md` | Adding a new project/layer, deciding where a class belongs, questions about dependency direction |
| `rules/coding-standards.md` | Writing/reviewing any C# code — naming, nullability, style |
| `rules/domain-rules.md` | Anything touching payroll calculation, the rule engine, multi-tenancy, or audit logging |
| `rules/cqrs-conventions.md` | Adding/modifying a Command, Query, Handler, or Validator |
| `rules/api-conventions.md` | Adding/modifying a Minimal API endpoint or DTO/response shape |
| `rules/testing.md` | Writing unit or integration tests |
| `rules/git-workflow.md` | Writing a commit message or naming a branch |

## 7. Language

All code, comments, identifiers, commit messages, and these rule files are in **English**, regardless of the language the developer talks to the AI in.