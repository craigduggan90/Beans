namespace Beans.Requestable;

/// <summary>Represents an error raised when trying to resolve the handler for a request.</summary>
/// <param name="message">A message describing the error.</param>
/// <param name="innerException">The exception that caused this exception, if applicable.</param>
public class RequestHandlerException(string message, Exception? innerException = null)
    : Exception(message, innerException)
{
    /// <summary>
    /// Creates an exception indicating that no handler could be resolved for the specified request type.
    /// </summary>
    /// <param name="requestType">The type of request for which the handler could not be resolved.</param>
    /// <returns>A <see cref="RequestHandlerException"/> describing the unresolved handler.</returns>
    internal static RequestHandlerException ForRequest(Type requestType) =>
        new($"Unable to resolve handler for '{requestType.Name}' request.");

    /// <summary>
    /// Creates an exception indicating that the handler wrapper for the specified request type could not be
    /// instantiated.
    /// </summary>
    /// <param name="requestType">The type of request for which the handler could not be instantiated.</param>
    /// <returns>A <see cref="RequestHandlerException"/> describing the failed instantiation.</returns>
    internal static RequestHandlerException ForFailedInstantiation(Type requestType) =>
        new($"Unable to instantiate handler for '{requestType.Name}' request.");
}