namespace Beans.Requestable.UnitTests.TestSupport;

internal sealed class EchoRequestHandler : IRequestHandler<EchoRequest, string>
{
    public CancellationToken? ReceivedToken { get; private set; }

    public Task<string> HandleAsync(EchoRequest request, CancellationToken cancellationToken)
    {
        ReceivedToken = cancellationToken;
        return Task.FromResult(request.Value);
    }
}

internal sealed class PingRequestHandler : IRequestHandler<PingRequest>
{
    public List<PingRequest> Received { get; } = [];

    public CancellationToken? ReceivedToken { get; private set; }

    public Task HandleAsync(PingRequest request, CancellationToken cancellationToken)
    {
        Received.Add(request);
        ReceivedToken = cancellationToken;
        return Task.CompletedTask;
    }
}

internal sealed class DualIntRequestHandler : IRequestHandler<DualRequest, int>
{
    public Task<int> HandleAsync(DualRequest request, CancellationToken cancellationToken) => Task.FromResult(42);
}

internal sealed class DualStringRequestHandler : IRequestHandler<DualRequest, string>
{
    public Task<string> HandleAsync(DualRequest request, CancellationToken cancellationToken)
        => Task.FromResult("forty-two");
}

/// <summary>Implements an unrelated, non-generic interface alongside the handler interface.</summary>
internal sealed class DisposableRequestHandler : IRequestHandler<DisposableRequest, string>, IDisposable
{
    public void Dispose()
    {
    }

    public Task<string> HandleAsync(DisposableRequest request, CancellationToken cancellationToken)
        => Task.FromResult("disposable");
}

/// <summary>Handles more than one request type.</summary>
internal sealed class MultiRequestHandler : IRequestHandler<FirstMultiRequest, string>, IRequestHandler<SecondMultiRequest>
{
    public Task<string> HandleAsync(FirstMultiRequest request, CancellationToken cancellationToken)
        => Task.FromResult("multi");

    public Task HandleAsync(SecondMultiRequest request, CancellationToken cancellationToken)
        => Task.CompletedTask;
}

internal abstract class AbstractRequestHandler : IRequestHandler<AbstractRequest, string>
{
    public abstract Task<string> HandleAsync(AbstractRequest request, CancellationToken cancellationToken);
}

internal sealed class GenericRequestHandler<T> : IRequestHandler<GenericRequest<T>, T>
{
    public Task<T> HandleAsync(GenericRequest<T> request, CancellationToken cancellationToken)
        => Task.FromResult(request.Value);
}

internal sealed class ThrowingRequestHandler : IRequestHandler<ThrowingRequest, string>
{
    public Task<string> HandleAsync(ThrowingRequest request, CancellationToken cancellationToken)
        => throw new InvalidOperationException("handler failed");
}