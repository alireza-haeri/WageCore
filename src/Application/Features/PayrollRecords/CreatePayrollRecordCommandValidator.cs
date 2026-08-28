namespace Application.Features.PayrollRecords;

public class CreatePayrollRecordCommandValidator : AbstractValidator<CreatePayrollRecordCommand>
{
    public CreatePayrollRecordCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("شناسه کاربر نمیتواند خالی باشد.");

        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("شناسه کارمند نمیتواند خالی باشد.");

        RuleFor(x => x.PersianYear)
            .GreaterThan(0).WithMessage("سال شمسی باید بزرگتر از صفر باشد.");

        RuleFor(x => x.PersianMonth)
            .GreaterThanOrEqualTo(1).WithMessage("ماه شمسی باید بین 1 تا 12 باشد.")
            .LessThanOrEqualTo(12).WithMessage("ماه شمسی نباید بیشتر از 12 باشد.");

        RuleFor(x => x.PayrollRecord)
            .NotNull().WithMessage("اطلاعات فیش پرداختی اجباری است.")
            .SetValidator(new PayrollRecordValidator());
    }
}
