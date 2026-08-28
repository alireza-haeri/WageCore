namespace Application.Features.PayrollRecords;

public class CreatePayrollRecordCommandValidator : AbstractValidator<CreatePayrollRecordCommand>
{
    public CreatePayrollRecordCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("شناسه کاربر نمیتواند خالی باشد.");

        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("شناسه کارمند نمیتواند خالی باشد.");

        RuleFor(x => x.PayrollRecord)
            .NotNull().WithMessage("اطلاعات فیش پرداختی اجباری است.")
            .SetValidator(new PayrollRecordValidator());
    }
}
