namespace Infrastructure.Repositories.CalculationFormulas;

public class CalculationFormulaRepository(WageCoreDbContext context, ILogger<CalculationFormulaRepository> logger)
    : ICalculationFormulaRepository
{
    public async Task<Guid?> CreateAsync(CalculationFormula formula, CancellationToken cancellationToken = default)
    {
        try
        {
            await context.CalculationFormulas.AddAsync(formula, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            return formula.Id;
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while creating a calculation formula for Key: {Key}.", formula.Key);
            return null;
        }
    }

    public async Task<CalculationFormula?> GetByIdAsync(Guid formulaId, CancellationToken cancellationToken = default)
    {
        return await context.CalculationFormulas
            .FirstOrDefaultAsync(x => x.Id == formulaId, cancellationToken);
    }

    public async Task<bool> UpdateAsync(CalculationFormula formula, CancellationToken cancellationToken = default)
    {
        try
        {
            context.CalculationFormulas.Update(formula);
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while updating a calculation formula for Id: {FormulaId}.", formula.Id);
            return false;
        }
    }

    public async Task<bool> DeleteAsync(Guid formulaId, CancellationToken cancellationToken = default)
    {
        try
        {
            var formula = await context.CalculationFormulas
                .FirstOrDefaultAsync(x => x.Id == formulaId, cancellationToken);
            if (formula is null)
                return false;

            context.CalculationFormulas.Remove(formula);
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while deleting a calculation formula for Id: {FormulaId}.", formulaId);
            return false;
        }
    }
}
