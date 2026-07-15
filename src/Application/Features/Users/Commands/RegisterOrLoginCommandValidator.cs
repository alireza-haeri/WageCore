namespace Application.Features.Users.Commands;

public class RegisterOrLoginCommandValidator : AbstractValidator<RegisterOrLoginCommand>
{
    public RegisterOrLoginCommandValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("{PropertyName} اجباری است.")
            .NotNull().WithMessage("{PropertyName} اجباری است.")
            .Length(11).WithMessage("{PropertyName} را درست وارد کنید.")
            .WithName("شماره تلفن");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("{PropertyName} اجباری است.")
            .NotNull().WithMessage("{PropertyName} اجباری است.")
            .MinimumLength(6).WithMessage("{PropertyName} نمیتواند کمتر از 6 کاراکتر باشد.")
            .MaximumLength(50).WithMessage("{PropertyName} نمیتواند بیشتر از 50 کاراکتر باشد.")
            .WithName("رمز عبور");
    }
}