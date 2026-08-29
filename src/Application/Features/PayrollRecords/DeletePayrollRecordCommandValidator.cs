namespace Application.Features.PayrollRecords;

public class DeletePayrollRecordCommandValidator : AbstractValidator<DeletePayrollRecordCommand>
{
    public DeletePayrollRecordCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("شناسه کاربر نمیتواند خالی باشد.");

        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("شناسه کارمند نمیتواند خالی باشد.");

        RuleFor(x => x.PayrollRecordId)
            .NotEmpty().WithMessage("شناسه فیش پرداختی نمیتواند خالی باشد.");
    }
}
