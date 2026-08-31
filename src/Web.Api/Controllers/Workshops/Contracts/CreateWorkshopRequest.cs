namespace Web.Api.Controllers.Workshops.Contracts;

public record CreateWorkshopRequest(
    string Name,
    string Address,
    PersianDate RegistrationDate,
    string NationalId,
    string SocialSecurityNumber,
    string? PostalCode = null,
    string? EconomicCode = null);