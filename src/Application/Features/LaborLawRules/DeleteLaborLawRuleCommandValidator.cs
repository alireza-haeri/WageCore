namespace Application.Features.LaborLawRules;

public class DeleteLaborLawRuleCommandValidator : AbstractValidator<DeleteLaborLawRuleCommand>
{
    public DeleteLaborLawRuleCommandValidator()
    {
        RuleFor(x => x.LaborLawRuleId)
            .NotEmpty().WithMessage("شناسه قانون اجباری است.");
    }
}
