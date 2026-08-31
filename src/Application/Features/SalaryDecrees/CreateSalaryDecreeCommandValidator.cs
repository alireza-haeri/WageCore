namespace Application.Features.SalaryDecrees;

public class CreateSalaryDecreeCommandValidator : AbstractValidator<CreateSalaryDecreeCommand>
{
    public CreateSalaryDecreeCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("شناسه کاربر نمیتواند خالی باشد.");

        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("شناسه کارمند نمیتواند خالی باشد.");

        RuleFor(x => x.SalaryProfile)
            .NotNull().WithMessage("اطلاعات پروفایل حقوق کارمند اجباری است.")
            .SetValidator(new SalaryDecreeValidator());
    }
}
