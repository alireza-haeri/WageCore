# Testing Rules

## Framework
- xUnit for test runner.
- FluentAssertions for assertions (`result.Should().Be(...)`), unless the project later standardizes on plain `Assert`.
- Moq (or NSubstitute — pick one and stay consistent) for mocking Core abstractions in Application-layer tests.

## What Gets Tested, and How Thoroughly

Priority order — payroll math and domain rules matter far more here than typical CRUD:

1. **Domain/rule-engine logic and payroll calculations — highest priority, highest coverage.** Every calculation path (tax, insurance, overtime, severance, proration, rounding) needs explicit test cases, including edge cases (zero values, partial months, boundary thresholds between tax brackets, etc.).
2. **Application Handlers** — test the orchestration logic with mocked Core abstractions: does the Handler call the right things, handle `Result` failures correctly, apply tenant scoping?
3. **Validators** — test that valid input passes and each invalid case is caught with the right error.
4. **API endpoints / integration** — test the happy path and key failure paths through `WebApplicationFactory`, primarily to catch wiring/DI/routing mistakes, not to re-test business logic already covered at the unit level.

Don't write tests for framework code, EF Core mapping configuration, or trivial property getters/setters.

## Naming

`MethodName_Scenario_ExpectedResult`, e.g.:
- `CalculateOvertimePay_WhenHoursExceedThreshold_AppliesMultiplier`
- `CreateEmployeeCommandHandler_WhenTenantMismatch_ReturnsFailure`

## Structure

Arrange / Act / Assert, with blank lines separating the three sections (no need to comment `// Arrange` etc. if the separation is already clear from spacing — but it's fine to add them for longer tests).

## Test Data

- Use builder/factory helpers for constructing test entities (e.g. `EmployeeBuilder`) instead of repeating long constructor calls across tests — especially useful given how many payroll-related fields an entity can have.
- Never hard-code "current date" logic in a way that makes a test's outcome depend on when it happens to run. Inject or fix the date/time (e.g. via an `IDateTimeProvider` abstraction) for anything date-sensitive, which is most of payroll.

## Multi-Tenancy in Tests

Always include at least one test per Handler/Query verifying that data belonging to another tenant is not returned/affected — this is a security boundary, not just a feature, and deserves explicit coverage rather than being assumed to work.

## Test Project Layout

Mirror the `src/` structure: `tests/Core.Tests`, `tests/Application.Tests`, `tests/Infrastructure.Tests` (if needed), `tests/Web.Api.IntegrationTests`.