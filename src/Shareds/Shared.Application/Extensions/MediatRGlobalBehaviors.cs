using System.Reflection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SamarPlanner.Shared.Application.Behaviors;

namespace SamarPlanner.Shared.Application.Extensions;

public static class MediatRGlobalBehaviors
{
    public static IServiceCollection AddGlobalBehaviors(this IServiceCollection services)
    {
        services.TryAddEnumerable(
            ServiceDescriptor.Transient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>)));

        return services;
    }
}