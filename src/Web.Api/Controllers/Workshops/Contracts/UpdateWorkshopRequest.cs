namespace Web.Api.Controllers.Workshops.Contracts;

public record UpdateWorkshopRequest(
    string Name,
    string Address,
    PersianDate RegistrationDate,
    string NationalId,
    string SocialSecurityNumber,
    string? PostalCode = null,
    string? EconomicCode = null);