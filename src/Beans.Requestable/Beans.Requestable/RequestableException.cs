namespace Beans.Requestable;

/// <summary>Represents an error raised by the Beans.Requestable package.</summary>
/// <param name="message">A message describing the error.</param>
/// <param name="innerException">The exception that caused this exception, if applicable.</param>
public class RequestableException(string message, Exception? innerException = null)
    : Exception(message, innerException)
{
    /// <summary>
    /// Creates an exception indicating that no handler could be resolved for the specified request type.
    /// </summary>
    /// <param name="requestType">The type of request for which the handler could not be resolved.</param>
    /// <returns>A <see cref="RequestableException"/> describing the unresolved handler.</returns>
    internal static RequestableException ForRequest(Type requestType) =>
        new($"Unable to resolve handler for '{requestType.Name}' request.");

    /// <summary>
    /// Creates an exception indicating that the handler wrapper for the specified request type could not be
    /// instantiated.
    /// </summary>
    /// <param name="requestType">The type of request for which the handler could not be instantiated.</param>
    /// <returns>A <see cref="RequestableException"/> describing the failed instantiation.</returns>
    internal static RequestableException ForFailedInstantiation(Type requestType) =>
        new($"Unable to instantiate handler for '{requestType.Name}' request.");

    /// <summary>
    /// Creates an exception indicating that more than one handler was found for the same request.
    /// </summary>
    /// <param name="duplicates">
    /// The duplicated handler interfaces, each grouped with the types that implement it.
    /// </param>
    /// <returns>A <see cref="RequestableException"/> describing every duplicated request and its handlers.</returns>
    internal static RequestableException ForDuplicateHandlers(IEnumerable<IGrouping<Type, Type>> duplicates)
    {
        // Sorted, so the message doesn't depend on the order that reflection happens to return types in.
        var details = duplicates
            .Select(group => (
                Request: group.Key.GenericTypeArguments[0].Name,
                Handlers: group.Select(handlerType => handlerType.FullName ?? handlerType.Name)
                    .Order(StringComparer.Ordinal)))
            .OrderBy(duplicate => duplicate.Request, StringComparer.Ordinal)
            .Select(duplicate => $"'{duplicate.Request}' request ({string.Join(", ", duplicate.Handlers)})");

        return new($"Unable to register request handlers: more than one handler found for {string.Join("; ", details)}.");
    }
}