using Beans.Common.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Beans.Common;

/// <summary>Service registration for the types in this package.</summary>
public static class DependencyInjection
{
    /// <summary>Registers Beans.Common services in this package with the container.</summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <returns>The same service collection, so calls can be chained.</returns>
    public static IServiceCollection AddCommonServices(this IServiceCollection services)
    {
        services.AddSingleton<IEnvironmentVariableAccessor, EnvironmentVariableAccessor>();
        return services;
    }
}