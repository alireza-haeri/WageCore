namespace Web.Api.Controllers.Users.Contracts;

public record LoginUserRequest(string? PhoneNumber, string? Email, string Password);
