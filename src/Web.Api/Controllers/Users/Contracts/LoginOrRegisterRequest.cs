namespace Web.Api.Controllers.Users.Contracts;

public record RegisterOrLoginRequest(string PhoneNumber, string Password);
