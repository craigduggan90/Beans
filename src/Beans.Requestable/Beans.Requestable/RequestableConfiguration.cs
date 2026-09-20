using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Beans.Requestable;

/// <summary>Configuration options for the requestable services.</summary>
public class RequestableConfiguration
{
    /// <summary>
    /// The assemblies from which to register request handlers.  When not set, the assembly that called
    /// <see cref="DependencyInjection.AddRequestableServices(IServiceCollection, Action{RequestableConfiguration})"/>
    /// is used.
    /// </summary>
    public IEnumerable<Assembly>? Assemblies { get; set; }

    /// <summary>
    /// The <see cref="IMediator"/> implementation to register.  Default is <see cref="Mediator"/>.  It must implement
    /// <see cref="IMediator"/>.
    /// </summary>
    public Type MediatorImplementationType { get; set; } = typeof(Mediator);

    /// <summary>
    /// The service lifetime of the mediator service.  Default is <see cref="ServiceLifetime.Transient"/>.
    /// </summary>
    public ServiceLifetime MediatorServiceLifetime { get; set; } = ServiceLifetime.Transient;
}