namespace Application.Features.PayrollRecords;

public class GetPayrollRecordCalculationDetailsQueryValidator : AbstractValidator<GetPayrollRecordCalculationDetailsQuery>
{
    public GetPayrollRecordCalculationDetailsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("شناسه کاربر نمیتواند خالی باشد.");

        RuleFor(x => x.PayrollRecordId)
            .NotEmpty().WithMessage("شناسه فیش پرداختی نمیتواند خالی باشد.");
    }
}
