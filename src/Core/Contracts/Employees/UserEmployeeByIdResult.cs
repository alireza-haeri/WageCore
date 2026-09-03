namespace Core.Contracts.Employees;

public record UserEmployeeByIdResult(
    Guid WorkshopId,
    Guid DepartmentId,
    string PersonalCode,
    string FullName,
    string NationalCode,
    string FatherName,
    EmployeeGender Gender,
    DateOnly HireDate,
    string PhoneNumber,
    string? JobTitle,
    Region Region,
    int? LeaveUsedInCurrentYear,
    int? NetWorkedDaysBeforeCurrentMonth,
    int? CarriedOverLeaveFromPreviousYear,
    List<EmployeeBankAccountDto> BankAccounts);
