using Core.Contracts.Employees;

namespace Application.Validations;

public class EmployeeBankAccountValidator : AbstractValidator<EmployeeBankAccountDto>
{
    public EmployeeBankAccountValidator()
    {
        RuleFor(x => x.Id)
            .NotEqual(Guid.Empty).WithMessage("شناسه حساب بانکی نمیتواند خالی باشد.")
            .When(x => x.Id.HasValue);

        RuleFor(x => x.Title)
            .MaximumLength(100).WithMessage("عنوان حساب بانکی نمیتواند بیشتر از 100 کاراکتر باشد.")
            .When(x => !string.IsNullOrWhiteSpace(x.Title));

        RuleFor(x => x.Iban)
            .NotEmpty().WithMessage("شماره شبا اجباری است.")
            .Matches(RegexExtensions.ValidIranianIbanRegex())
            .WithMessage("شماره شبا باید با IR شروع شود و پس از آن 24 رقم انگلیسی داشته باشد.");
    }
}
