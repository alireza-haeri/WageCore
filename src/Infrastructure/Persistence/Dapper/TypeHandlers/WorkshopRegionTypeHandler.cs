namespace Infrastructure.Persistence.Dapper.TypeHandlers;

public class WorkshopRegionTypeHandler : SqlMapper.TypeHandler<WorkshopRegion>
{
    public override void SetValue(IDbDataParameter parameter, WorkshopRegion value)
    {
        parameter.Value = value.ToString();
    }

    public override WorkshopRegion Parse(object value)
    {
        return Enum.Parse<WorkshopRegion>((string)value);
    }
}