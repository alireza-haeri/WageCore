namespace Infrastructure.Persistence.Dapper.TypeHandlers;

public class RegionTypeHandler : SqlMapper.TypeHandler<Region>
{
    public override void SetValue(IDbDataParameter parameter, Region value)
    {
        parameter.Value = value.ToString();
    }

    public override Region Parse(object value)
    {
        return Enum.Parse<Region>((string)value);
    }
}