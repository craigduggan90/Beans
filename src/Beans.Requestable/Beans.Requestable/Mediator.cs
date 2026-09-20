using Beans.Requestable.Wrappers;
using System.Collections.Concurrent;

namespace Beans.Requestable;

/// <summary>Service responsible for sending requests to their handlers.</summary>
/// <param name="serviceProvider">The service provider used to resolve handlers.</param>
public class Mediator(IServiceProvider serviceProvider) : IMediator
{
    // The wrappers hold no state (the service provider is passed in per call), so one instance per request type is
    // enough, and saves reflecting and allocating on every send.
    private static readonly ConcurrentDictionary<Type, RequestHandlerWrapperBase> VoidWrappers = new();

    /// <inheritdoc />
    public Task SendAsync(IRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // We don't know what type of IRequest is provided, so we need to work out the appropriate wrapper type (i.e.
        // RequestHandlerWrapper<MyRequest> where MyRequest : IRequest).  The wrapper is boxed as its base type,
        // RequestHandlerWrapperBase, since the request type can't be passed as a generic type parameter here.
        var wrapper = VoidWrappers.GetOrAdd(request.GetType(), static requestType =>
            Activator.CreateInstance(typeof(RequestHandlerWrapper<>).MakeGenericType(requestType))
                as RequestHandlerWrapperBase
            ?? throw RequestHandlerException.ForFailedInstantiation(requestType));

        // The base type declares HandleAsync(IRequest, IServiceProvider, CancellationToken); the derived wrapper
        // implements it against the concrete request type.
        return wrapper.HandleAsync(request, serviceProvider, cancellationToken);
    }

    /// <inheritdoc />
    public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var wrapper = ResponseWrappers<TResponse>.Instances.GetOrAdd(request.GetType(), static requestType =>
            Activator.CreateInstance(typeof(RequestHandlerWrapper<,>).MakeGenericType(requestType, typeof(TResponse)))
                as RequestHandlerWrapperBase<TResponse>
            ?? throw RequestHandlerException.ForFailedInstantiation(requestType));

        return wrapper.HandleAsync(request, serviceProvider, cancellationToken);
    }

    // One cache per response type, so a request type that implements IRequest<A> and IRequest<B> gets a wrapper for
    // each.
    private static class ResponseWrappers<TResponse>
    {
        public static readonly ConcurrentDictionary<Type, RequestHandlerWrapperBase<TResponse>> Instances = new();
    }
}