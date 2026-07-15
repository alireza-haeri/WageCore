using FluentValidation;
using MediatR;
using SamarPlanner.Shared.Kernel;
using System.Reflection;

namespace SamarPlanner.Shared.Application.Behaviors;

public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (typeof(TResponse).IsGenericType && 
            typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            if (!validators.Any())
                return await next();

            var context = new ValidationContext<TRequest>(request);
            var results = await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            var failures = results.SelectMany(r => r.Errors).Where(f => f != null).ToList();

            if (failures.Any())
            {
                var errorDict = failures
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                var resultType = typeof(TResponse); 
                var validationFailureMethod = resultType.GetMethod(
                    "ValidationFailure",
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    [typeof(IDictionary<string, string[]>)],
                    null);
                
                if (validationFailureMethod == null)
                    throw new InvalidOperationException($"Method ValidationFailure not found on {resultType}");
                
                var failureResult = validationFailureMethod.Invoke(null, [errorDict]);
                return (TResponse)failureResult!;
            }
        }

        return await next(); 
    }
}