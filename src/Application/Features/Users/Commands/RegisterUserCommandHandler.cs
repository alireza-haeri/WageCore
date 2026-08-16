namespace Application.Features.Users.Commands;

public class RegisterUserCommandHandler(
    IUserRepository userRepository,
    IJwtTokenService jwtToken)
    : IRequestHandler<RegisterUserCommand, Result<RegisterUserCommandResponse>>
{
    public async Task<Result<RegisterUserCommandResponse>> Handle(RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        if (await userRepository.ExistsAsync(request.PhoneNumber, request.Email, cancellationToken))
            return Result<RegisterUserCommandResponse>.ValidationFailure(new Dictionary<string, string[]>
            {
                { "PhoneNumber", ["کاربری با شماره تلفن مورد نظر یافت شد!"] },
                { "Email", ["کاربری با ایمیل مورد نظر یافت شد!"] }
            });

        var user = User.Create(request.PhoneNumber, request.Email, request.FullName);
        if (!user.IsSuccess)
            return Result<RegisterUserCommandResponse>.GeneralFailure(user.ErrorMessage!);

        var createResult = await userRepository.CreateAsync(user.Response!, request.Password, cancellationToken);
        if (!createResult.Succeeded)
            if (createResult.Errors.Any())
                return Result<RegisterUserCommandResponse>.ValidationFailure(createResult.Errors);
            else
                return Result<RegisterUserCommandResponse>.GeneralFailure("خطایی در ایجاد کاربر رخ داد");

        var token = jwtToken.GenerateToken(user.Response!);

        return Result<RegisterUserCommandResponse>.Success(
            new RegisterUserCommandResponse
            (
                token.Token,
                token.ExpiresInMinutes
            ));
    }
}