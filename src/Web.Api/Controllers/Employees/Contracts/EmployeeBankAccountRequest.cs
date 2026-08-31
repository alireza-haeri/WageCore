namespace Web.Api.Controllers.Employees.Contracts;

public record EmployeeBankAccountRequest(
    string? BankName,
    string? BranchCode,
    string Iban,
    Guid? Id = null);

public record EmployeeBankAccountResponse(
    string? BankName,
    string? BranchCode,
    string Iban,
    Guid? Id = null);
