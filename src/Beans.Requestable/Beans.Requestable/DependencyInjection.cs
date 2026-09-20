using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Beans.Requestable;

/// <summary>Service registration for the types in Beans.Requestable.</summary>
public static class DependencyInjection
{
    private static readonly Type[] HandlerInterfaces = [typeof(IRequestHandler<>), typeof(IRequestHandler<,>)];

    /// <summary>
    /// Registers <see cref="IMediator"/> and the request handlers found in the calling assembly with the DI container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection, so calls can be chained.</returns>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static IServiceCollection AddRequestableServices(this IServiceCollection services)
        => services.Register(new RequestableConfiguration { Assemblies = [Assembly.GetCallingAssembly()] });

    /// <summary>
    /// Registers <see cref="IMediator"/> and the request handlers with the DI container using a custom configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Configures the registration.</param>
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

        services.AddHandlers(handlerTypes);

        services.Add(new ServiceDescriptor(
            typeof(IMediator),
            configuration.MediatorImplementationType,
            configuration.MediatorServiceLifetime));

        return services;
    }

    /// <summary>Registers each request handler interface implemented by the given types as a transient service.</summary>
    /// <param name="services">The service collection to add the handlers to.</param>
    /// <param name="handlerTypes">The concrete, closed types to register handlers from.</param>
    /// <returns>The same service collection, so calls can be chained.</returns>
    /// <exception cref="RequestableException">
    /// Thrown when more than one of the types handles the same request.  Nothing is registered in that case.
    /// </exception>
    internal static IServiceCollection AddHandlers(this IServiceCollection services, IEnumerable<Type> handlerTypes)
    {
        // A type can implement several handler interfaces (and any number of unrelated ones), so pair each handler
        // interface with the type that implements it.
        var registrations = handlerTypes
            .Distinct()
            .SelectMany(handlerType => handlerType.GetInterfaces()
                .Where(i => i.IsGenericType && HandlerInterfaces.Contains(i.GetGenericTypeDefinition()))
                .Select(serviceType => (ServiceType: serviceType, HandlerType: handlerType)))
            .ToList();

        // Only what was found here is compared, so a handler registered by hand afterwards can still replace one.
        var duplicates = registrations
            .GroupBy(registration => registration.ServiceType, registration => registration.HandlerType)
            .Where(group => group.Count() > 1)
            .ToList();

        if (duplicates.Count > 0)
            throw RequestableException.ForDuplicateHandlers(duplicates);

        foreach (var (serviceType, handlerType) in registrations)
            services.AddTransient(serviceType, handlerType);

        return services;
    }
}