namespace Infrastructure.Persistence.Dapper.TypeHandlers;

public class FormulaKeyTypeHandler : SqlMapper.TypeHandler<FormulaKey>
{
    public override void SetValue(IDbDataParameter parameter, FormulaKey value)
    {
        parameter.Value = value.ToString();
    }

    public override FormulaKey Parse(object value)
    {
        return Enum.Parse<FormulaKey>((string)value);
    }
}
