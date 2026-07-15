

namespace Application.Features.User.Commands;

public sealed record RegisterOrLoginCommand(string PhoneNumber, string Password)
    : IRequest<Result<RegisterOrLoginCommandResponse>>;

public sealed record RegisterOrLoginCommandResponse(string Token, int ExpireInMinutes);