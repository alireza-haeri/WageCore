using Core.Contracts.Employees;

namespace Application.Validations;

public class EmployeeBankAccountValidator : AbstractValidator<EmployeeBankAccountDto>
{
    public EmployeeBankAccountValidator()
    {
        RuleFor(x => x.Id)
            .NotEqual(Guid.Empty).WithMessage("شناسه حساب بانکی نمیتواند خالی باشد.")
            .When(x => x.Id.HasValue);

        RuleFor(x => x.BankName)
            .MaximumLength(100).WithMessage("نام بانک نمیتواند بیشتر از 100 کاراکتر باشد.")
            .When(x => !string.IsNullOrWhiteSpace(x.BankName));

        RuleFor(x => x.BranchCode)
            .MaximumLength(100).WithMessage("کد شعبه نمیتواند بیشتر از 100 کاراکتر باشد.")
            .When(x => !string.IsNullOrWhiteSpace(x.BranchCode));

        RuleFor(x => x.Iban)
            .NotEmpty().WithMessage("شماره شبا اجباری است.")
            .Matches(RegexExtensions.ValidIranianIbanRegex())
            .WithMessage("شماره شبا باید با IR شروع شود و پس از آن 24 رقم انگلیسی داشته باشد.");
    }
}
