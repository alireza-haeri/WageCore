using Shared.Application.Behaviors;

namespace Shared.Application.Extensions;

public static class MediatRGlobalBehaviors
{
    public static IServiceCollection AddGlobalBehaviors(this IServiceCollection services)
    {
        services.TryAddEnumerable(
            ServiceDescriptor.Transient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>)));

        return services;
    }
}