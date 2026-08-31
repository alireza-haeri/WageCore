namespace Core.Contracts.Employees;

public record EmployeeBankAccountDto(
    string? BankName,
    string? BranchCode,
    string Iban,
    Guid? Id = null);
