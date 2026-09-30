using Microsoft.Extensions.DependencyInjection;

namespace Beans.Cacheable;

/// <summary>Service registration for the types in Beans.Cacheable.</summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers <see cref="IDistributedCacheClient"/> with the DI container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection, so calls can be chained.</returns>
    /// <remarks>
    /// An <c>IDistributedCache</c> must already be registered, e.g. via <c>AddDistributedMemoryCache</c> or a
    /// provider-specific extension such as <c>AddStackExchangeRedisCache</c>.  Registering a
    /// <see cref="System.Text.Json.JsonSerializerOptions"/> singleton is optional; when present, it is used to
    /// serialize cached values instead of the built-in default.
    /// </remarks>
    public static IServiceCollection AddCacheableServices(this IServiceCollection services)
    {
        services.AddSingleton<IDistributedCacheClient, DistributedCacheClient>();
        return services;
    }
}