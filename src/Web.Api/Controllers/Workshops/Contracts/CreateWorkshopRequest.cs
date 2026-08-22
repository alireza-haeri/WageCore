namespace Web.Api.Controllers.Workshops.Contracts;

public record CreateWorkshopRequest(
    string Name,
    string Address,
    WorkshopRegion Region,
    PersianDate RegistrationDate,
    string NationalId,
    string? PostalCode = null);