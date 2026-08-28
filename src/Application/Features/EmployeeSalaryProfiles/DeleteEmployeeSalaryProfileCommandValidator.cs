namespace Application.Features.EmployeeSalaryProfiles;

public class DeleteEmployeeSalaryProfileCommandValidator : AbstractValidator<DeleteEmployeeSalaryProfileCommand>
{
    public DeleteEmployeeSalaryProfileCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("شناسه کاربر نمیتواند خالی باشد.");

        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("شناسه کارمند نمیتواند خالی باشد.");

        RuleFor(x => x.EmployeeSalaryProfileId)
            .NotEmpty().WithMessage("شناسه پروفایل حقوق کارمند نمیتواند خالی باشد.");
    }
}
