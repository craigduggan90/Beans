namespace Beans.Requestable;

/// <summary>Defines a mediator to handle routing of requests to handlers.</summary>
public interface IMediator
{
    /// <summary>Send a request to a handler without a response.</summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the send operation.</returns>
    Task SendAsync(IRequest request, CancellationToken cancellationToken);

    /// <summary>Send a request to a handler.</summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <typeparam name="TResponse">The type of response from the handler.</typeparam>
    /// <returns>A task that represents the send operation. The task result contains the handler response.</returns>
    Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken);
}