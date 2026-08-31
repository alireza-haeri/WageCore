namespace Web.Api.Controllers.Workshops.Contracts;

public record GetWorkshopForEditResponse(
    string Name,
    string Address,
    string RegistrationDate,
    string NationalId,
    string? PostalCode = null,
    string SocialSecurityNumber = null!,
    string? EconomicCode = null);