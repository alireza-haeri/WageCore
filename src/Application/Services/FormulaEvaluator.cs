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
                    if (!TryAddParameter(parameters, variable.Name, NormalizeParameterValue(variable.Value, null)))
                        return FailureForDuplicatedParameter(expression, variable.Name);

                    continue;
                }

                var modelType = modelOrVariable.GetType();

                foreach (var property in modelType.GetProperties())
                {
                    var parameterName = $"{modelType.Name}{property.Name}";

                    if (!TryAddParameter(parameters, parameterName, NormalizeParameterValue(property.GetValue(modelOrVariable), property.PropertyType)))
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
    // so [DateOnly] = [DateOnly] fails at evaluation time. Normalize them to DateTime
    // (a type NCalc compares by value) so equality checks work inside expressions.
    // Nullable numeric model properties (decimal?, int?, ...) are normalized to their
    // default (0) so a formula can reference them even when the source value is null.
    // declaredType is null for explicit FormulaVariable values, whose null-ness cannot
    // be attributed to a nullable numeric type at runtime.
    private static object? NormalizeParameterValue(object? value, Type? declaredType)
    {
        if (value is null &&
            declaredType is not null &&
            TryGetNullableNumericUnderlyingType(declaredType, out var numericType))
            return Activator.CreateInstance(numericType);

        return value switch
        {
            DateOnly dateOnly => dateOnly.ToDateTime(TimeOnly.MinValue),
            TimeOnly timeOnly => new DateTime(1, 1, 1, timeOnly.Hour, timeOnly.Minute, timeOnly.Second, timeOnly.Millisecond),
            _ => value
        };
    }

    private static bool TryGetNullableNumericUnderlyingType(Type type, out Type numericType)
    {
        numericType = Nullable.GetUnderlyingType(type) ?? type;

        return numericType == typeof(decimal)
            || numericType == typeof(double)
            || numericType == typeof(float)
            || numericType == typeof(int)
            || numericType == typeof(long)
            || numericType == typeof(short)
            || numericType == typeof(byte)
            || numericType == typeof(sbyte)
            || numericType == typeof(uint)
            || numericType == typeof(ulong)
            || numericType == typeof(ushort);
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
            or DateTime
            or DateOnly
            or TimeOnly;
}
