using Core.Domain.Enums;

namespace Shared.Tests.Builders;

public class CalculationFormulaBuilder
{
    private Guid _id = Guid.NewGuid();
    private FormulaKey _key = FormulaKey.OvertimePay;
    private string _expression = "OvertimeHours * HourlyRate * 1.4";
    private DateOnly? _effectiveFrom = DateOnly.FromDateTime(DateTime.Now.AddDays(-30));

    public CalculationFormulaBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public CalculationFormulaBuilder WithKey(FormulaKey key)
    {
        _key = key;
        return this;
    }

    public CalculationFormulaBuilder WithExpression(string expression)
    {
        _expression = expression;
        return this;
    }

    public CalculationFormulaBuilder WithEffectiveFrom(DateOnly? effectiveFrom)
    {
        _effectiveFrom = effectiveFrom;
        return this;
    }

    public DomainResult<CalculationFormula> CreateResult()
    {
        return CalculationFormula.Create(_id, _key, _expression, _effectiveFrom);
    }
}
