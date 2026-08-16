namespace Application.Features.Users.Commands;

public record RegisterUserCommand(string? PhoneNumber, string? Email, string FullName, string Password) : IRequest<Result<RegisterUserCommandResponse>>;
public record RegisterUserCommandResponse(string Token, int ExpireInMinutes);