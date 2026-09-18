using Microsoft.Extensions.DependencyInjection;

namespace DanmaobTisax.Application;

/// <summary>
/// Extension type for adding Application layer services to <see cref="IServiceCollection"/>.
/// Registering this assembly does not automatically register application services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds the Application layer infrastructure (DI registration) to the service collection.
    /// Application-layer logic should be registered directly within <see cref="Program"/> or a startup file,
    /// by registering interfaces and implementations explicitly. This method ensures that any shared
    /// Application infrastructure can be included with minimal configuration in future updates.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services) => services;
}