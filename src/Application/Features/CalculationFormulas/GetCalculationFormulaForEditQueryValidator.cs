namespace Application.Features.CalculationFormulas;

public class GetCalculationFormulaForEditQueryValidator : AbstractValidator<GetCalculationFormulaForEditQuery>
{
    public GetCalculationFormulaForEditQueryValidator()
    {
        RuleFor(x => x.CalculationFormulaId)
            .NotEmpty().WithMessage("شناسه فرمول نمیتواند خالی باشد.");
    }
}
