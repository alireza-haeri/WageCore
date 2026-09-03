using Infrastructure.Persistence.Dapper;

namespace Infrastructure.Repositories.CalculationFormulas;

public class CalculationFormulaQuery(IDbConnectionFactory dbConnectionFactory) : ICalculationFormulaQuery
{
    public async Task<string?> GetActiveExpressionAsync(
        FormulaKey key, DateOnly date, CancellationToken cancellationToken = default)
    {
        string sql = $"""
                      SELECT TOP 1 f.Expression
                      FROM {CalculationFormula.TableName} f
                      WHERE f.[Key] = @Key
                      AND f.EffectiveFrom <= @Date
                      ORDER BY f.EffectiveFrom DESC, f.Id DESC;
                      """;

        var command = new CommandDefinition(sql, new
        {
            Key = key.ToString(),
            Date = date
        }, cancellationToken: cancellationToken);

        using var connection = dbConnectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<string?>(command);
    }

    public async Task<PagedResult<CalculationFormulaResult>> GetCalculationFormulasAsync(
        PaginationDto pagination,
        FormulaKey? key = null,
        CancellationToken cancellationToken = default)
    {
        string sql = $"""
                      SELECT
                          f.Id,
                          f.[Key],
                          f.Expression,
                          f.EffectiveFrom
                      FROM {CalculationFormula.TableName} f
                      WHERE (@Key IS NULL OR f.[Key] = @Key)
                      ORDER BY f.EffectiveFrom DESC, f.Id DESC
                      OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                      SELECT COUNT(*)
                      FROM {CalculationFormula.TableName} f
                      WHERE (@Key IS NULL OR f.[Key] = @Key);
                      """;

        var command = new CommandDefinition(sql, new
        {
            Key = key?.ToString(),
            Offset = pagination.Offset,
            PageSize = pagination.PageSize
        }, cancellationToken: cancellationToken);

        using var connection = dbConnectionFactory.CreateConnection();
        await using var multi = await connection.QueryMultipleAsync(command);

        var formulas = (await multi.ReadAsync<CalculationFormulaResult>()).ToList();
        var totalCount = await multi.ReadSingleAsync<int>();

        return new PagedResult<CalculationFormulaResult>(formulas, totalCount, pagination.PageNumber, pagination.PageSize);
    }

    public async Task<CalculationFormulaByIdResult?> GetCalculationFormulaByIdAsync(
        Guid formulaId,
        CancellationToken cancellationToken = default)
    {
        string sql = $"""
                      SELECT
                          f.[Key],
                          f.Expression,
                          f.EffectiveFrom
                      FROM {CalculationFormula.TableName} f
                      WHERE f.Id = @FormulaId;
                      """;

        var command = new CommandDefinition(sql, new { FormulaId = formulaId }, cancellationToken: cancellationToken);

        using var connection = dbConnectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<CalculationFormulaByIdResult>(command);
    }

    public async Task<bool> IsExistEffectiveFrom(
        FormulaKey key,
        DateOnly effectiveFrom,
        Guid? excludeFormulaId = null,
        CancellationToken cancellationToken = default)
    {
        string sql = $"""
                      SELECT CASE WHEN EXISTS (
                          SELECT 1
                          FROM {CalculationFormula.TableName} f
                          WHERE f.EffectiveFrom = @EffectiveFrom AND f.[Key] = @Key
                          AND (@ExcludeFormulaId IS NULL OR f.Id <> @ExcludeFormulaId)
                      ) THEN 1 ELSE 0 END
                      """;

        var command = new CommandDefinition(sql, new
        {
            Key = key.ToString(),
            EffectiveFrom = effectiveFrom,
            ExcludeFormulaId = excludeFormulaId
        }, cancellationToken: cancellationToken);

        using var connection = dbConnectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<bool>(command);
    }
}
