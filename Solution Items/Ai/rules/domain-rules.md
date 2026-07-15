# Domain Rules

These are the rules specific to the payroll domain. Read this before touching anything related to payroll calculation, tenants, or auditing.

## 1. The Rule Engine

Payroll rules that change over time by law or company policy — insurance percentage, tax brackets, overtime multiplier, severance (سنوات) formula, holiday bonus (عیدی) — must **never** be hard-coded as literals in Application or Core logic.

- These values live in configuration/rule data (database-backed or config-backed — check current implementation in `Infrastructure` before assuming), not in C# constants.
- When a calculation needs one of these values, it goes through `IRuleEngine` (or the relevant Core abstraction), not a direct lookup.
- When adding a new payroll rule, ask: "will this need to change without a redeploy when the law changes next year?" If yes, it belongs in the rule engine, not in code.
- Effective-dating matters: rules can change year to year, and historical payslips must still be recalculable/auditable using the rule version that was active at the time. Don't silently apply "current" rules to a past period unless that's explicitly what's being asked.

## 2. Multi-Tenancy

Every business using WageCore is a tenant. Tenant isolation is a **hard security boundary**, not a convenience feature.

- Every entity that holds tenant-specific data implements the tenant-scoping marker (e.g. `ITenantScoped`) and carries a `TenantId`.
- Every query against tenant-scoped data must be filtered by the current tenant — via EF Core global query filters at the `DbContext` level, not manually re-added in every handler. If you're writing a query and don't see a global filter already handling it, flag this rather than assuming it's fine.
- Never accept a `TenantId` from client input for authorization purposes — it must come from the authenticated user's context (`ITenantProvider` resolved from the JWT/claims), never from a request body/query string that the caller controls.
- Cross-tenant data leakage is treated as a critical/security bug, not a normal bug.

## 3. Audit Log

Any change to salary amounts or personnel data must be recorded.

- An audit entry captures: what changed (entity + field-level if feasible), old value → new value, who made the change, timestamp (UTC), and the reason if one was provided.
- Audit logging happens through `IAuditLogger` (or the equivalent Core abstraction) — don't write ad hoc logging calls scattered through handlers.
- Audit entries are immutable once written — never update or delete an audit record.
- When adding a new Command that mutates payroll or personnel data, always ask whether it needs an audit entry. Default assumption: **it does**, unless it's a pure read or a non-sensitive operational change.

## 4. Event-Driven Flow

- Attendance-related actions (absence, overtime, leave) raise domain events rather than directly mutating payroll totals inline.
- Payroll recalculation reacts to these events asynchronously rather than being called synchronously from the attendance use case. Keep this separation — don't collapse it into a single synchronous call chain "for simplicity."

## 5. Background Jobs

- End-of-month payslip generation is a scheduled background job, not something triggered manually per employee in the normal flow.
- Background jobs must be idempotent — re-running a month's payslip generation job should not create duplicate payslips or double-apply audit entries.

## 6. Money and Rounding

- All monetary values are `decimal`.
- Rounding happens at well-defined points (e.g. final payslip line totals), using a single centralized rounding rule — not inconsistently across different calculation steps.
- When in doubt about a rounding or proration rule for a specific payroll component, ask rather than guessing — these numbers affect real people's paychecks.