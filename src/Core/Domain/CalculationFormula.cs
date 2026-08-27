namespace Core.Domain;

public class CalculationFormula
{
    public const string TableName = "CalculationFormulas";

    public Guid Id { get; private init; }
    public FormulaKey Key { get; private set; }
    public string Expression { get; private set; } = null!;
    public DateOnly EffectiveFrom { get; private set; }

    public static DomainResult<CalculationFormula> Create(
        Guid formulaId, FormulaKey key, string expression, DateOnly? effectiveFrom)
    {
        if (formulaId == Guid.Empty)
            return DomainResult<CalculationFormula>.Failure("شناسه فرمول نمیتواند خالی باشد.");

        var validationResult = Validate(expression, effectiveFrom);
        if (!validationResult.IsSuccess)
            return DomainResult<CalculationFormula>.Failure(validationResult.ErrorMessage!);

        return DomainResult<CalculationFormula>.Success(new CalculationFormula
        {
            Id = formulaId,
            Key = key,
            Expression = expression,
            EffectiveFrom = effectiveFrom!.Value
        });
    }

    public static DomainResult<CalculationFormula> Create(
        FormulaKey key, string expression, DateOnly? effectiveFrom) =>
        Create(Guid.NewGuid(), key, expression, effectiveFrom);

    public DomainResult Update(FormulaKey key, string expression, DateOnly? effectiveFrom)
    {
        var validationResult = Validate(expression, effectiveFrom);
        if (!validationResult.IsSuccess)
            return validationResult;

        Key = key;
        Expression = expression;
        EffectiveFrom = effectiveFrom!.Value;

        return DomainResult.Success();
    }

    private static DomainResult Validate(string expression, DateOnly? effectiveFrom)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return DomainResult.Failure("عبارت فرمول نمیتواند خالی باشد.");

        if (effectiveFrom is null)
            return DomainResult.Failure("تاریخ اجرا نمیتواند خالی باشد.");

        return DomainResult.Success();
    }
}
