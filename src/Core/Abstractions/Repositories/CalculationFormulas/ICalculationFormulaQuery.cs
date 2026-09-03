namespace Core.Abstractions.Repositories.CalculationFormulas;

public interface ICalculationFormulaQuery
{
    Task<string?> GetActiveExpressionAsync(
        FormulaKey key, DateOnly date, CancellationToken cancellationToken = default);

    Task<PagedResult<CalculationFormulaResult>> GetCalculationFormulasAsync(
        PaginationDto pagination,
        FormulaKey? key = null,
        CancellationToken cancellationToken = default);

    Task<CalculationFormulaByIdResult?> GetCalculationFormulaByIdAsync(
        Guid formulaId,
        CancellationToken cancellationToken = default);

    Task<bool> IsExistEffectiveFrom(
        FormulaKey key,
        DateOnly effectiveFrom,
        Guid? excludeFormulaId = null,
        CancellationToken cancellationToken = default);
}
