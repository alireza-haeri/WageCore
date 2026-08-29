using Microsoft.Extensions.Logging;
using NCalc;

namespace Application.Services;

public class FormulaEvaluator(ILogger<FormulaEvaluator> logger) : IFormulaEvaluator
{
    public DomainResult<decimal> Evaluate(string expression, params object[] models)
    {
        try
        {
            var expr = new Expression(expression);

            foreach (var model in models)
            {
                foreach (var property in model.GetType().GetProperties())
                {
                    var value = property.GetValue(model);

                    if (value is not null && IsSupportedType(value))
                        expr.Parameters[property.Name] = value;
                }
            }

            return DomainResult<decimal>.Success(Convert.ToDecimal(expr.Evaluate()));
        }
        catch (Exception e)
        {
            logger.LogError(e, "The formula {Expression} could not be evaluated", expression);
            return DomainResult<decimal>.Failure($"خطا در محاسبه‌ی فرمول: {e.Message}");
        }
    }

    private static bool IsSupportedType(object value) =>
        value is decimal
            or double
            or float
            or int
            or uint
            or long
            or ulong
            or short
            or ushort
            or byte
            or sbyte
            or bool
            or string
            or DateTime;
}
