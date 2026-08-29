namespace Core.Abstractions.Services;

public interface IFormulaEvaluator
{
    DomainResult<decimal> Evaluate(string expression, params object[] models);
}
