using Infrastructure.Persistence.Dapper.TypeHandlers;

namespace Infrastructure.Persistence.Dapper;

public static class DapperTypeHandlers
{
    public static void Register()
    {
        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
        SqlMapper.AddTypeHandler(new RegionTypeHandler());
        SqlMapper.AddTypeHandler(new LaborLawRuleKeyTypeHandler());
        SqlMapper.AddTypeHandler(new FormulaKeyTypeHandler());
    }
}
