namespace Application.Features.Users.Commands;

public record LoginUserCommand(string? PhoneNumber, string? Email, string Password) : IRequest<Result<LoginUserCommandResponse>>;
public record LoginUserCommandResponse(string Token, int ExpireInMinutes);