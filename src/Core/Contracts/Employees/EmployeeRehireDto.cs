namespace Core.Contracts.Employees;

public record EmployeeRehireDto(
    Guid DepartmentId,
    DateOnly? WorkshopRegistrationDate,
    DateOnly? HireDate);
