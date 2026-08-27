namespace Application.Features.EmployeeSalaryProfiles;

public class UpdateEmployeeSalaryProfileCommandValidator : AbstractValidator<UpdateEmployeeSalaryProfileCommand>
{
    public UpdateEmployeeSalaryProfileCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("شناسه کاربر نمیتواند خالی باشد.");

        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("شناسه کارمند نمیتواند خالی باشد.");

        RuleFor(x => x.EmployeeSalaryProfileId)
            .NotEmpty().WithMessage("شناسه پروفایل حقوق کارمند نمیتواند خالی باشد.");

        RuleFor(x => x.SalaryProfile)
            .NotNull().WithMessage("اطلاعات پروفایل حقوق کارمند اجباری است.")
            .SetValidator(new EmployeeSalaryProfileValidator());
    }
}
