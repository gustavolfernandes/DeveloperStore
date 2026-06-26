using Ambev.DeveloperEvaluation.Application.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Ambev.DeveloperEvaluation.Application;

/// <summary>
/// Registers the Application layer services (MediatR handlers and the validation pipeline).
/// AutoMapper profiles are registered by the host so it can scan every assembly that
/// contributes profiles (Application and WebApi) in a single configuration.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
