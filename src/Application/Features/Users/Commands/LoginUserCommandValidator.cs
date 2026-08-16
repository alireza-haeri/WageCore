namespace Application.Features.Users.Commands;

public class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.PhoneNumber) || !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("حداقل یکی از فیلدهای شماره تلفن یا ایمیل باید وارد شود.");

        RuleFor(x => x.PhoneNumber)
            .Matches(RegexExtensions.ValidIranianPhoneNumberRegex())
            .WithMessage("شماره تلفن باید با ۰۹ شروع شده و دقیقاً ۱۱ رقم انگلیسی باشد.")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

        RuleFor(x => x.Email)
            .Matches(RegexExtensions.ValidEmailRegex()).WithMessage("فرمت ایمیل را درست وارد کنید.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("{PropertyName} اجباری است.")
            .MinimumLength(6).WithMessage("{PropertyName} نمیتواند کمتر از 6 کاراکتر باشد.")
            .MaximumLength(50).WithMessage("{PropertyName} نمیتواند بیشتر از 50 کاراکتر باشد.")
            .WithName("رمز عبور");
    }
}