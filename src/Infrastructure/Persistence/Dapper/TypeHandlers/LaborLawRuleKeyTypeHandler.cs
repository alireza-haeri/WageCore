namespace Infrastructure.Persistence.Dapper.TypeHandlers;

public class LaborLawRuleKeyTypeHandler : SqlMapper.TypeHandler<LaborLawRuleKey>
{
    public override void SetValue(IDbDataParameter parameter, LaborLawRuleKey value)
    {
        parameter.Value = value.ToString();
    }

    public override LaborLawRuleKey Parse(object value)
    {
        return Enum.Parse<LaborLawRuleKey>((string)value);
    }
}
