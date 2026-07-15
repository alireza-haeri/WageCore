namespace Application.Features.Users.Commands;

public sealed record RegisterOrLoginCommand(string PhoneNumber, string Password)
    : IRequest<Result<RegisterOrLoginCommandResponse>>;

public sealed record RegisterOrLoginCommandResponse(string Token, int ExpireInMinutes);