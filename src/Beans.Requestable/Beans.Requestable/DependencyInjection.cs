using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Beans.Requestable;

/// <summary>Service registration for the types in this package.</summary>
public static class DependencyInjection
{
    private static readonly Type[] HandlerInterfaces = [typeof(IRequestHandler<>), typeof(IRequestHandler<,>)];

    /// <summary>
    /// Registers <see cref="IMediator"/> and the request handlers found in the calling assembly with the container.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <returns>The same service collection, so calls can be chained.</returns>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static IServiceCollection AddRequestableServices(this IServiceCollection services)
        => services.Register(new RequestableConfiguration { Assemblies = [Assembly.GetCallingAssembly()] });

    /// <summary>
    /// Registers <see cref="IMediator"/> and the request handlers with the container, using a custom configuration.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <param name="configure">
    /// Configures the registration.  If it does not set <see cref="RequestableConfiguration.Assemblies"/>, the calling
    /// assembly is searched for request handlers.
    /// </param>
    /// <returns>The same service collection, so calls can be chained.</returns>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static IServiceCollection AddRequestableServices(
        this IServiceCollection services,
        Action<RequestableConfiguration> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var configuration = new RequestableConfiguration();
        configure(configuration);
        configuration.Assemblies ??= [Assembly.GetCallingAssembly()];

        return services.Register(configuration);
    }

    private static IServiceCollection Register(this IServiceCollection services, RequestableConfiguration configuration)
    {
        if (!typeof(IMediator).IsAssignableFrom(configuration.MediatorImplementationType))
        {
            throw new ArgumentException(
                $"'{configuration.MediatorImplementationType.Name}' does not implement {nameof(IMediator)}.",
                nameof(configuration));
        }

        var handlerTypes = (configuration.Assemblies ?? [])
            .Distinct()
            .SelectMany(assembly => assembly.DefinedTypes)
            .Where(type => type is { IsAbstract: false, IsInterface: false, ContainsGenericParameters: false });

        foreach (var handlerType in handlerTypes)
        {
            // A type can implement several handler interfaces (and any number of unrelated ones), so register each
            // handler interface it implements.
            var serviceTypes = handlerType.ImplementedInterfaces
                .Where(i => i.IsGenericType && HandlerInterfaces.Contains(i.GetGenericTypeDefinition()));

            foreach (var serviceType in serviceTypes)
                services.AddTransient(serviceType, handlerType);
        }

        services.Add(new ServiceDescriptor(
            typeof(IMediator),
            configuration.MediatorImplementationType,
            configuration.MediatorServiceLifetime));

        return services;
    }
}