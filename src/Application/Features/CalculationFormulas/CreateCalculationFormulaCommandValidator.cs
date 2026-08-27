namespace Application.Features.CalculationFormulas;

public class CreateCalculationFormulaCommandValidator : AbstractValidator<CreateCalculationFormulaCommand>
{
    public CreateCalculationFormulaCommandValidator()
    {
        RuleFor(x => x.Key)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("کلید فرمول اجباری است.")
            .Must(x => x.HasValue && Enum.IsDefined(x.Value))
            .WithMessage("کلید فرمول معتبر نیست.");

        RuleFor(x => x.Expression)
            .NotEmpty().WithMessage("عبارت فرمول اجباری است.");

        RuleFor(x => x.EffectiveFrom)
            .NotEmpty().WithMessage("تاریخ اجرا اجباری است.");
    }
}
