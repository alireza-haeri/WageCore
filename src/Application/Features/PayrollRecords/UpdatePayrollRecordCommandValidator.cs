namespace Application.Features.PayrollRecords;

public class UpdatePayrollRecordCommandValidator : AbstractValidator<UpdatePayrollRecordCommand>
{
    public UpdatePayrollRecordCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("شناسه کاربر نمیتواند خالی باشد.");

        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("شناسه کارمند نمیتواند خالی باشد.");

        RuleFor(x => x.PayrollRecordId)
            .NotEmpty().WithMessage("شناسه فیش پرداختی نمیتواند خالی باشد.");

        RuleFor(x => x.PersianYear)
            .GreaterThan(0).WithMessage("سال شمسی باید بزرگتر از صفر باشد.");

        RuleFor(x => x.PersianMonth)
            .GreaterThanOrEqualTo(1).WithMessage("ماه شمسی باید بین 1 تا 12 باشد.")
            .LessThanOrEqualTo(12).WithMessage("ماه شمسی نباید بیشتر از 12 باشد.");

        RuleFor(x => x.Work)
            .NotNull().WithMessage("اطلاعات کارکرد کارمند اجباری است.")
            .SetValidator(new UserWorkInputValidator());
    }
}