namespace Core.Contracts.Employees;

public record EmployeeDto(
    Guid DepartmentId,
    string PersonalCode,
    string FullName,
    string NationalCode,
    string FatherName,
    EmployeeGender? Gender,
    DateOnly? HireDate,
    string PhoneNumber,
    string? JobTitle,
    Region? Region,
    decimal? LeaveUsedInCurrentYear = null,
    decimal? NetWorkedDaysBeforeCurrentMonth = null,
    decimal? CarriedOverLeaveFromPreviousYear = null);
