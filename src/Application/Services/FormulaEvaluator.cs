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
                var modelType = model.GetType();

                foreach (var property in modelType.GetProperties())
                {
                    var value = property.GetValue(model);

                    if (value is null || !IsSupportedType(value))
                        continue;

                    var parameterName = $"{modelType.Name}{property.Name}";

                    if (expr.Parameters.ContainsKey(parameterName))
                        return FailureForDuplicatedParameter(expression, parameterName);

                    expr.Parameters[parameterName] = value;
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

    private DomainResult<decimal> FailureForDuplicatedParameter(string expression, string parameterName)
    {
        logger.LogError(
            "The formula {Expression} received the parameter {Parameter} more than once",
            expression,
            parameterName);

        return DomainResult<decimal>.Failure($"نام پارامتر {parameterName} در فرمول تکراری است.");
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
