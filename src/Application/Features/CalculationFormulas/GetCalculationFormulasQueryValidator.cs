namespace Application.Features.CalculationFormulas;

public class GetCalculationFormulasQueryValidator : AbstractValidator<GetCalculationFormulasQuery>
{
    public GetCalculationFormulasQueryValidator()
    {
        RuleFor(x => x.Pagination)
            .SetValidator(new PaginationDtoValidator());

        RuleFor(x => x.Key)
            .Must(x => x.HasValue && Enum.IsDefined(x.Value))
            .WithMessage("کلید فرمول معتبر نیست.")
            .When(x => x.Key.HasValue);
    }
}
