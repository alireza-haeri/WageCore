namespace Core.Abstractions.Repositories.CalculationFormulas;

public interface ICalculationFormulaRepository
{
    Task<Guid?> CreateAsync(CalculationFormula formula, CancellationToken cancellationToken = default);
    Task<CalculationFormula?> GetByIdAsync(Guid formulaId, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(CalculationFormula formula, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid formulaId, CancellationToken cancellationToken = default);
}
