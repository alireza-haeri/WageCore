namespace Web.Api.Controllers.Employees.Contracts;

public record EmployeeInformationRequest(
    Guid DepartmentId,
    string PersonalCode,
    string FullName,
    string NationalCode,
    string FatherName,
    EmployeeGender Gender,
    PersianDate HireDate,
    string PhoneNumber,
    string? JobTitle,
    Region Region,
    int? LeaveUsedInCurrentYear = null,
    int? NetWorkedDaysBeforeCurrentMonth = null,
    int? CarriedOverLeaveFromPreviousYear = null);
