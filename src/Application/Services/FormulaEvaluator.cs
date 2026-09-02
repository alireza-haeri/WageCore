using Core.Contracts.CalculationFormulas;
using Microsoft.Extensions.Logging;
using NCalc;

namespace Application.Services;

public class FormulaEvaluator(ILogger<FormulaEvaluator> logger) : IFormulaEvaluator
{
    public DomainResult<decimal> Evaluate(string expression, params object[] modelsAndVariables)
    {
        try
        {
            var parameters = new Dictionary<string, object>();

            foreach (var modelOrVariable in modelsAndVariables)
            {
                if (modelOrVariable is FormulaVariable variable)
                {
                    if (!TryAddParameter(parameters, variable.Name, NormalizeParameterValue(variable.Value)))
                        return FailureForDuplicatedParameter(expression, variable.Name);

                    continue;
                }

                var modelType = modelOrVariable.GetType();

                foreach (var property in modelType.GetProperties())
                {
                    var parameterName = $"{modelType.Name}{property.Name}";

                    if (!TryAddParameter(parameters, parameterName, NormalizeParameterValue(property.GetValue(modelOrVariable))))
                        return FailureForDuplicatedParameter(expression, parameterName);
                }
            }

            var expr = new Expression(expression);
            foreach (var parameter in parameters)
                expr.Parameters[parameter.Key] = parameter.Value;

            return DomainResult<decimal>.Success(Convert.ToDecimal(expr.Evaluate()));
        }
        catch (Exception e)
        {
            logger.LogError(e, "The formula {Expression} could not be evaluated", expression);
            return DomainResult<decimal>.Failure($"خطا در محاسبه‌ی فرمول: {e.Message}");
        }
    }

    private static bool TryAddParameter(IDictionary<string, object> parameters, string name, object? value) =>
        value is null || !IsSupportedType(value) || parameters.TryAdd(name, value);

    // NCalc's equality/comparison operators do not understand DateOnly/TimeOnly natively,
    // so IF([DateOnly] = [DateOnly]) fails at evaluation time. Normalize them to DateTime
    // (a type NCalc compares by value) so equality checks work inside expressions.
    private static object? NormalizeParameterValue(object? value) =>
        value switch
        {
            DateOnly dateOnly => dateOnly.ToDateTime(TimeOnly.MinValue),
            TimeOnly timeOnly => new DateTime(1, 1, 1, timeOnly.Hour, timeOnly.Minute, timeOnly.Second, timeOnly.Millisecond),
            _ => value
        };

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
            or DateTime
            or DateOnly
            or TimeOnly;
}
