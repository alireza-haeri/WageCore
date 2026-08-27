namespace Application.Features.LaborLawRules;

public class GetLaborLawRuleForEditQueryValidator : AbstractValidator<GetLaborLawRuleForEditQuery>
{
    public GetLaborLawRuleForEditQueryValidator()
    {
        RuleFor(x => x.LaborLawRuleId)
            .NotEmpty().WithMessage("شناسه قانون نمیتواند خالی باشد.");
    }
}
