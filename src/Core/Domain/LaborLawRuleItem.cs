namespace Core.Domain;

public class LaborLawRuleItem
{
    public const string TableName = "LaborLawRuleItems";

    public Guid Id { get; private init; }
    public LaborLawRuleKey Key { get; private set; }
    public decimal Value { get; private set; }
    public DateOnly EffectiveFrom { get; private set; }

    public static DomainResult<LaborLawRuleItem> Create(
        Guid ruleId, LaborLawRuleKey key, decimal value, DateOnly? effectiveFrom)
    {
        if (ruleId == Guid.Empty)
            return DomainResult<LaborLawRuleItem>.Failure("شناسه قانون نمیتواند خالی باشد.");

        var validationResult = Validate(value, effectiveFrom);
        if (!validationResult.IsSuccess)
            return DomainResult<LaborLawRuleItem>.Failure(validationResult.ErrorMessage!);

        return DomainResult<LaborLawRuleItem>.Success(new LaborLawRuleItem
        {
            Id = ruleId,
            Key = key,
            Value = value,
            EffectiveFrom = effectiveFrom!.Value
        });
    }

    public static DomainResult<LaborLawRuleItem> Create(
        LaborLawRuleKey key, decimal value, DateOnly? effectiveFrom) =>
        Create(Guid.NewGuid(), key, value, effectiveFrom);

    public DomainResult Update(LaborLawRuleKey key, decimal value, DateOnly? effectiveFrom)
    {
        var validationResult = Validate(value, effectiveFrom);
        if (!validationResult.IsSuccess)
            return validationResult;

        Key = key;
        Value = value;
        EffectiveFrom = effectiveFrom!.Value;

        return DomainResult.Success();
    }

    private static DomainResult Validate(decimal value, DateOnly? effectiveFrom)
    {
        if (value < 0)
            return DomainResult.Failure("مقدار قانون نمیتواند منفی باشد.");

        if (effectiveFrom is null)
            return DomainResult.Failure("تاریخ اجرا نمیتواند خالی باشد.");

        return DomainResult.Success();
    }
}
