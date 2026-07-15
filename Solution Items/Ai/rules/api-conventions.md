# API Conventions

Web.Api uses **MVC Controllers** (`ControllerBase`), grouped by feature. Controllers are thin — they translate HTTP in and out, and delegate everything else to MediatR.

## Controller Organization

- One controller per feature/resource: `EmployeesController`, `PayrollController`, `AttendanceController`.
- Controllers live under `Web.Api/Controllers/`.
- Inject `IMediator` via the constructor (primary constructor syntax). No business logic in the controller — every action builds a Command/Query, sends it via `IMediator`, and maps the `Result<T>` to an `IActionResult`.

## Routing

- Attribute routing on every controller: `[Route("api/v{version:apiVersion}/[controller]")]`.
- Use plural nouns for resource collections: `/employees`, `/payslips`, not `/employee`, `/payslip`.
- Standard REST verbs where the operation maps naturally (`GET /employees/{id}`, `POST /employees`); for operations that don't map to CRUD (e.g. "calculate payroll for month"), use a clear action-style sub-route: `POST /payroll/{month}/calculate`.
- Action methods are named after what they do (`GetById`, `Create`, `CalculateForMonth`), not after the HTTP verb alone.

## Request/Response Shape

- Actions accept/return DTOs from Core, never EF Core entities directly.
- Action body: build the Command/Query → send via `IMediator` → map `Result<T>` to an HTTP response. No business logic in the action itself.
- Success: return the appropriate `Ok(...)/CreatedAtAction(...)/NoContent()` wrapping the DTO.
- Failure: map `Result` failures to `ProblemDetails` consistently — this mapping logic lives once (e.g. a base controller method or an action filter in `Shared.Web`), not duplicated per action.
- Prefer a small `ApiControllerBase` (in `Shared.Web` or `Web.Api`) with a shared `HandleResult(Result<T>)` helper so every controller maps `Result` → HTTP the same way.

## Authentication & Authorization

- `[Authorize]` at the controller level by default; explicitly mark the rare exceptions (e.g. login) with `[AllowAnonymous]`.
- Tenant resolution happens from the authenticated user's claims — never from a route/query parameter.
- Role/permission checks use `[Authorize(Policy = "...")]` with policies defined centrally, not ad hoc checks inside the action body.

## Versioning

- Use `Asp.Versioning` (or the project's chosen API versioning package) with the version in the route (`/api/v1/...`). Introduce `/api/v2/...` only for breaking changes to a specific feature, not the whole API at once.

## Validation Errors

- Validation failures from the MediatR pipeline surface as `400 Bad Request` with a `ProblemDetails` body listing field-level errors — this is handled once by the shared `Result` → `ProblemDetails` mapping, not per controller.
- Enable `[ApiController]` on every controller to get automatic model-state validation for malformed requests, in addition to the FluentValidation pipeline behavior for Command/Query-level validation.

## OpenAPI/Swagger

- Every action should have `[ProducesResponseType]` attributes for its possible outcomes, and a summary/description where the shape isn't obvious — future-you (or a client dev) will read this without full context.