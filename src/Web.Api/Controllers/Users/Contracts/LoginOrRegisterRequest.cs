namespace Web.Api.Controllers.Users.Contracts;

public record RegisterUserRequest(string? PhoneNumber, string? Email, string FullName, string Password);
