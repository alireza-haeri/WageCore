namespace Application.Features.SalaryDecrees;

public class UpdateSalaryDecreeCommandValidator : AbstractValidator<UpdateSalaryDecreeCommand>
{
    public UpdateSalaryDecreeCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("شناسه کاربر نمیتواند خالی باشد.");

        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("شناسه کارمند نمیتواند خالی باشد.");

        RuleFor(x => x.SalaryDecreeId)
            .NotEmpty().WithMessage("شناسه پروفایل حقوق کارمند نمیتواند خالی باشد.");

        RuleFor(x => x.SalaryProfile)
            .NotNull().WithMessage("اطلاعات پروفایل حقوق کارمند اجباری است.")
            .SetValidator(new SalaryDecreeValidator());
    }
}
