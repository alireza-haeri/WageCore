namespace Application.Features.Employees;

public class TerminateEmployeeCommandValidator : AbstractValidator<TerminateEmployeeCommand>
{
    public TerminateEmployeeCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("شناسه کاربر نمیتواند خالی باشد.");

        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("شناسه کارمند نمیتواند خالی باشد.");

        RuleFor(x => x.TerminationDate)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("تاریخ ترک کار اجباری است.")
            .Must(x => x.HasValue && x.Value <= DateOnly.FromDateTime(DateTime.Now))
            .WithMessage("تاریخ ترک کار نباید برای آینده باشد.");
    }
}
