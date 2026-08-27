using Infrastructure.Persistence.Dapper;

namespace Infrastructure.Repositories.LaborLaw;

public class LaborLawRuleQuery(IDbConnectionFactory dbConnectionFactory) : ILaborLawRuleQuery
{
    public async Task<decimal?> GetActiveValueAsync(
        LaborLawRuleKey key, DateOnly date, CancellationToken cancellationToken = default)
    {
        string sql = $"""
                      SELECT TOP 1 r.Value
                      FROM {LaborLawRuleItem.TableName} r
                      WHERE r.[Key] = @Key
                      AND r.EffectiveFrom <= @Date
                      ORDER BY r.EffectiveFrom DESC, r.Id DESC;
                      """;

        var command = new CommandDefinition(sql, new
        {
            Key = key.ToString(),
            Date = date
        }, cancellationToken: cancellationToken);

        using var connection = dbConnectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<decimal?>(command);
    }

    public async Task<PagedResult<LaborLawRuleResult>> GetLaborLawRulesAsync(
        PaginationDto pagination,
        LaborLawRuleKey? key = null,
        CancellationToken cancellationToken = default)
    {
        string sql = $"""
                      SELECT
                          r.Id,
                          r.[Key],
                          r.Value,
                          r.EffectiveFrom
                      FROM {LaborLawRuleItem.TableName} r
                      WHERE (@Key IS NULL OR r.[Key] = @Key)
                      ORDER BY r.EffectiveFrom DESC, r.Id DESC
                      OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                      SELECT COUNT(*)
                      FROM {LaborLawRuleItem.TableName} r
                      WHERE (@Key IS NULL OR r.[Key] = @Key);
                      """;

        var command = new CommandDefinition(sql, new
        {
            Key = key?.ToString(),
            Offset = pagination.Offset,
            PageSize = pagination.PageSize
        }, cancellationToken: cancellationToken);

        using var connection = dbConnectionFactory.CreateConnection();
        await using var multi = await connection.QueryMultipleAsync(command);

        var rules = (await multi.ReadAsync<LaborLawRuleResult>()).ToList();
        var totalCount = await multi.ReadSingleAsync<int>();

        return new PagedResult<LaborLawRuleResult>(rules, totalCount, pagination.PageNumber, pagination.PageSize);
    }

    public async Task<LaborLawRuleByIdResult?> GetLaborLawRuleByIdAsync(
        Guid ruleId,
        CancellationToken cancellationToken = default)
    {
        string sql = $"""
                      SELECT
                          r.[Key],
                          r.Value,
                          r.EffectiveFrom
                      FROM {LaborLawRuleItem.TableName} r
                      WHERE r.Id = @RuleId;
                      """;

        var command = new CommandDefinition(sql, new { RuleId = ruleId }, cancellationToken: cancellationToken);

        using var connection = dbConnectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<LaborLawRuleByIdResult>(command);
    }
}
