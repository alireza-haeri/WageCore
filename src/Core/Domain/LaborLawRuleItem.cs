namespace Core.Domain;

public class LaborLawRuleItem
{
    public const string TableName = "LaborLawRuleItems";

    public Guid Id { get; private init; }
    public LaborLawRuleKey Key { get; private init; }
    public decimal Value { get; private init; }
    public DateOnly EffectiveFrom { get; private init; }

    public static DomainResult<LaborLawRuleItem> Create(
        Guid ruleId, LaborLawRuleKey key, decimal value, DateOnly? effectiveFrom)
    {
        if (ruleId == Guid.Empty)
            return DomainResult<LaborLawRuleItem>.Failure("شناسه قانون نمیتواند خالی باشد.");

        if (value < 0)
            return DomainResult<LaborLawRuleItem>.Failure("مقدار قانون نمیتواند منفی باشد.");

        if (effectiveFrom is null)
            return DomainResult<LaborLawRuleItem>.Failure("تاریخ اجرا نمیتواند خالی باشد.");

        return DomainResult<LaborLawRuleItem>.Success(new LaborLawRuleItem
        {
            Id = ruleId,
            Key = key,
            Value = value,
            EffectiveFrom = effectiveFrom.Value
        });
    }

    public static DomainResult<LaborLawRuleItem> Create(
        LaborLawRuleKey key, decimal value, DateOnly? effectiveFrom) =>
        Create(Guid.NewGuid(), key, value, effectiveFrom);
}
