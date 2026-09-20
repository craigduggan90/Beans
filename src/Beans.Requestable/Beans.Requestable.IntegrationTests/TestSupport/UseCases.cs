namespace Beans.Requestable.IntegrationTests.TestSupport;

public sealed record GreetRequest(string Name) : IRequest<string>;

public sealed record LogRequest(string Message) : IRequest;

public sealed record WhoAmIRequest : IRequest<Guid>;

public sealed record FailRequest : IRequest<string>;

public sealed record UnhandledRequest : IRequest<string>;

/// <summary>A scoped service, so requests can prove they share a scope with the controller that sent them.</summary>
public sealed class RequestScope
{
    public Guid Id { get; } = Guid.NewGuid();
}

/// <summary>A singleton that records what void requests received.</summary>
public sealed class MessageLog
{
    private readonly List<string> _messages = [];

    public IReadOnlyList<string> Messages
    {
        get
        {
            lock (_messages)
                return [.. _messages];
        }
    }

    public void Add(string message)
    {
        lock (_messages)
            _messages.Add(message);
    }
}

public sealed class GreetRequestHandler : IRequestHandler<GreetRequest, string>
{
    public Task<string> HandleAsync(GreetRequest request, CancellationToken cancellationToken)
        => Task.FromResult($"Hello, {request.Name}!");
}

public sealed class LogRequestHandler(MessageLog log) : IRequestHandler<LogRequest>
{
    public Task HandleAsync(LogRequest request, CancellationToken cancellationToken)
    {
        log.Add(request.Message);
        return Task.CompletedTask;
    }
}

public sealed class WhoAmIRequestHandler(RequestScope scope) : IRequestHandler<WhoAmIRequest, Guid>
{
    public Task<Guid> HandleAsync(WhoAmIRequest request, CancellationToken cancellationToken)
        => Task.FromResult(scope.Id);
}

public sealed class FailRequestHandler : IRequestHandler<FailRequest, string>
{
    public Task<string> HandleAsync(FailRequest request, CancellationToken cancellationToken)
        => throw new InvalidOperationException("handler failed");
}