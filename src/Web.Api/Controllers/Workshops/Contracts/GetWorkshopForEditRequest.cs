namespace Web.Api.Controllers.Workshops.Contracts;

public record GetWorkshopForEditResponse(
    string Name,
    string Address,
    WorkshopRegion Region,
    string RegistrationDate,
    string NationalId,
    string? PostalCode = null);