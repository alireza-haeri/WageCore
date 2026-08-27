namespace Application.Features.Users.Commands;

public class LoginUserCommandHandler(IUserRepository userRepository, IJwtTokenService jwtTokenService)
    : IRequestHandler<LoginUserCommand, Result<LoginUserCommandResponse>>
{
    public async Task<Result<LoginUserCommandResponse>> Handle(LoginUserCommand request,
        CancellationToken cancellationToken)
    {
        bool userExists = true;

        var user = await userRepository.GetAsync(request.PhoneNumber, request.Email, cancellationToken);
        if (user is null)
            userExists = false;
        else
        {
            var checkPasswordResult =
                await userRepository.CheckPasswordAsync(user, request.Password, cancellationToken);
            if (!checkPasswordResult)
                userExists = false;
        }

        if (!userExists)
            return Result<LoginUserCommandResponse>.ValidationFailure(new Dictionary<string, string[]>
            {
                { "PhoneNumber", ["شماره تلفن یا رمز عبور اشتباه است!"] },
                { "Email", ["ایمیل یا رمز عبور اشتباه است!"] }
            });

        var roles = await userRepository.GetRolesAsync(user!.Id, cancellationToken);
        var token = jwtTokenService.GenerateToken(user, roles);
        return Result<LoginUserCommandResponse>.Success(
            new LoginUserCommandResponse
            (
                token.Token,
                token.ExpiresInMinutes
            ));
    }
}