namespace Application.Features.LaborLawRules;

public class GetLaborLawRulesQueryValidator : AbstractValidator<GetLaborLawRulesQuery>
{
    public GetLaborLawRulesQueryValidator()
    {
        RuleFor(x => x.Pagination)
            .SetValidator(new PaginationDtoValidator());

        RuleFor(x => x.Key)
            .Must(x => x.HasValue && Enum.IsDefined(x.Value))
            .WithMessage("کلید قانون معتبر نیست.")
            .When(x => x.Key.HasValue);
    }
}
