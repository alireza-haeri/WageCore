namespace Application.Features.CalculationFormulas;

public class DeleteCalculationFormulaCommandValidator : AbstractValidator<DeleteCalculationFormulaCommand>
{
    public DeleteCalculationFormulaCommandValidator()
    {
        RuleFor(x => x.CalculationFormulaId)
            .NotEmpty().WithMessage("شناسه فرمول اجباری است.");
    }
}
