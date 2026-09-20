namespace Beans.Requestable;

/// <summary>Marker interface for a request which does not return a value.</summary>
public interface IRequest : IRequestBase;

/// <summary>Marker interface for a request which returns <typeparamref name="TResponse"/>.</summary>
/// <typeparam name="TResponse">The response type.</typeparam>
public interface IRequest<TResponse> : IRequestBase;

/// <summary>Generic marker for a request type.</summary>
public interface IRequestBase;