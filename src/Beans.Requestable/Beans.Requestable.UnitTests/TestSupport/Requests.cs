namespace Beans.Requestable.UnitTests.TestSupport;

internal sealed record EchoRequest(string Value) : IRequest<string>;

internal sealed record PingRequest : IRequest;

/// <summary>A request that can be answered with either an <see cref="int"/> or a <see cref="string"/>.</summary>
internal sealed record DualRequest : IRequest<int>, IRequest<string>;

internal sealed record DisposableRequest : IRequest<string>;

internal sealed record FirstMultiRequest : IRequest<string>;

internal sealed record SecondMultiRequest : IRequest;

internal sealed record AbstractRequest : IRequest<string>;

internal sealed record GenericRequest<T>(T Value) : IRequest<T>;

internal sealed record UnhandledRequest : IRequest<string>;

internal sealed record UnhandledVoidRequest : IRequest;

internal sealed record ThrowingRequest : IRequest<string>;

// Duplicate handlers are generic so the assembly scan skips them, otherwise every test that scans this assembly would
// fail.  Tests hand closed versions (e.g. FirstDuplicateHandler<int>) to the registration directly.
internal sealed record DuplicateRequest<T> : IRequest<string>;

internal sealed record DuplicateVoidRequest<T> : IRequest;