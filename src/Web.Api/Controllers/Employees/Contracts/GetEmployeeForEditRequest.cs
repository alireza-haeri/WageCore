namespace Web.Api.Controllers.Employees.Contracts;

public record GetEmployeeForEditResponse(
    Guid WorkshopId,
    Guid DepartmentId,
    string PersonalCode,
    string FullName,
    string NationalCode,
    string FatherName,
    EmployeeGender Gender,
    string HireDate,
    string PhoneNumber,
    string? JobTitle,
    Region Region,
    List<EmployeeBankAccountResponse> BankAccounts);
