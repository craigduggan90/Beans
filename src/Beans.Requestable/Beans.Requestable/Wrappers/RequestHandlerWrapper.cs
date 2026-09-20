namespace Beans.Requestable.Wrappers;

/// <summary>
/// Describes a generic handler wrapper for an instance of <typeparamref name="TRequest"/> without return.
/// </summary>
/// <typeparam name="TRequest">The type of request handled.</typeparam>
internal sealed class RequestHandlerWrapper<TRequest> : RequestHandlerWrapperBase
    where TRequest : IRequest
{
    /// <inheritdoc />
    public override async Task HandleAsync(
        IRequest request,
        IServiceProvider provider,
        CancellationToken cancellationToken)
    {
        var handler = provider.GetService(typeof(IRequestHandler<TRequest>)) as IRequestHandler<TRequest>
                      ?? throw RequestableException.ForRequest(typeof(TRequest));

        await handler.HandleAsync((TRequest)request, cancellationToken).ConfigureAwait(false);
    }
}

/// <summary>Describes a generic handler wrapper for an instance of <typeparamref name="TRequest"/>.</summary>
/// <typeparam name="TRequest">The type of request handled.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
internal sealed class RequestHandlerWrapper<TRequest, TResponse> : RequestHandlerWrapperBase<TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <inheritdoc />
    public override async Task<TResponse> HandleAsync(
        IRequest<TResponse> request,
        IServiceProvider provider,
        CancellationToken cancellationToken)
    {
        var handler = provider.GetService(typeof(IRequestHandler<TRequest, TResponse>))
                          as IRequestHandler<TRequest, TResponse>
                      ?? throw RequestableException.ForRequest(typeof(TRequest));

        return await handler.HandleAsync((TRequest)request, cancellationToken).ConfigureAwait(false);
    }
}