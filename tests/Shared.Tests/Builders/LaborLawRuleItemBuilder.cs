namespace Shared.Tests.Builders;

public class LaborLawRuleItemBuilder
{
    private Guid _id = Guid.NewGuid();
    private LaborLawRuleKey _key = LaborLawRuleKey.MinimumMonthlySalary;
    private decimal _value = 71_661_840m;
    private DateOnly? _effectiveFrom = DateOnly.FromDateTime(DateTime.Now.AddDays(-30));

    public LaborLawRuleItemBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public LaborLawRuleItemBuilder WithKey(LaborLawRuleKey key)
    {
        _key = key;
        return this;
    }

    public LaborLawRuleItemBuilder WithValue(decimal value)
    {
        _value = value;
        return this;
    }

    public LaborLawRuleItemBuilder WithEffectiveFrom(DateOnly? effectiveFrom)
    {
        _effectiveFrom = effectiveFrom;
        return this;
    }

    public DomainResult<LaborLawRuleItem> CreateResult()
    {
        return LaborLawRuleItem.Create(_id, _key, _value, _effectiveFrom);
    }
}
