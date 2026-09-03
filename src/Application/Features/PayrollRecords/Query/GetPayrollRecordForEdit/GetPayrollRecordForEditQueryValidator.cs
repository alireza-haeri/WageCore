namespace Application.Features.PayrollRecords;

public class GetPayrollRecordForEditQueryValidator : AbstractValidator<GetPayrollRecordForEditQuery>
{
    public GetPayrollRecordForEditQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("شناسه کاربر نمیتواند خالی باشد.");

        RuleFor(x => x.PayrollRecordId)
            .NotEmpty().WithMessage("شناسه فیش پرداختی نمیتواند خالی باشد.");
    }
}
