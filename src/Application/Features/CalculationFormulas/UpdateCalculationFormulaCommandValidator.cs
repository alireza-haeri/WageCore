namespace Application.Features.CalculationFormulas;

public class UpdateCalculationFormulaCommandValidator : AbstractValidator<UpdateCalculationFormulaCommand>
{
    public UpdateCalculationFormulaCommandValidator()
    {
        RuleFor(x => x.CalculationFormulaId)
            .NotEmpty().WithMessage("شناسه فرمول نمیتواند خالی باشد.");

        RuleFor(x => x.Key)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("کلید فرمول اجباری است.")
            .Must(x => x.HasValue && Enum.IsDefined(x.Value))
            .WithMessage("کلید فرمول معتبر نیست.");

        RuleFor(x => x.Expression)
            .NotEmpty().WithMessage("عبارت فرمول اجباری است.")
            .MaximumLength(2000).WithMessage("عبارت فرمول نمیتواند بیشتر از 2000 کاراکتر باشد.");

        RuleFor(x => x.EffectiveFrom)
            .NotEmpty().WithMessage("تاریخ اجرا اجباری است.");
    }
}
