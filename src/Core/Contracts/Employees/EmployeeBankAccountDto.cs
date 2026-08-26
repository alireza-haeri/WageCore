namespace Core.Contracts.Employees;

public record EmployeeBankAccountDto(string? Title, string Iban, Guid? Id = null);
