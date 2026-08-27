namespace Application.Features.LaborLawRules;

public class UpdateLaborLawRuleCommandValidator : AbstractValidator<UpdateLaborLawRuleCommand>
{
    public UpdateLaborLawRuleCommandValidator()
    {
        RuleFor(x => x.LaborLawRuleId)
            .NotEmpty().WithMessage("شناسه قانون نمیتواند خالی باشد.");

        RuleFor(x => x.Key)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("کلید قانون اجباری است.")
            .Must(x => x.HasValue && Enum.IsDefined(x.Value))
            .WithMessage("کلید قانون معتبر نیست.");

        RuleFor(x => x.Value)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("مقدار قانون اجباری است.")
            .GreaterThanOrEqualTo(0).WithMessage("مقدار قانون نمیتواند منفی باشد.");

        RuleFor(x => x.EffectiveFrom)
            .NotEmpty().WithMessage("تاریخ اجرا اجباری است.");
    }
}
