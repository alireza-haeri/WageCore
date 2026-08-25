namespace Web.Api.Controllers.Workshops.Contracts;

public record UpdateWorkshopRequest(
    string Name,
    string Address,
    WorkshopRegion Region,
    PersianDate RegistrationDate,
    string NationalId,
    string? PostalCode = null);