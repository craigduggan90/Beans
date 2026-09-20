using Beans.Common.Providers;

namespace Beans.Pageable;

/// <summary>
/// Generates cursors for <see cref="IHasCursor"/> entities from the time they are created.  Two entities can be created
/// within the same microsecond (or under a fixed <see cref="DateTimeOffsetProvider"/> time), so a cursor is never the same as, or
/// lower than, one this class has already handed out.
/// </summary>
/// <remarks>
/// Cursors are only unique within a process.  If more than one instance of an application creates entities, consider
/// either using an identity-based cursor, or adding a unique index to the persisted cursor column.  
/// </remarks>
public static class CursorGenerator
{
    private static readonly CursorSequence Sequence = new();

    /// <summary>Gets the next cursor, based on <see cref="DateTimeOffsetProvider.UtcNow"/>.</summary>
    /// <returns>A cursor greater than any previously returned by this class.</returns>
    public static long Next() => Next(DateTimeOffsetProvider.UtcNow);

    /// <summary>Gets the next cursor, based on a timestamp.</summary>
    /// <param name="timestamp">The time the entity was created.</param>
    /// <returns>
    /// A cursor greater than any previously returned by this class: the timestamp as microseconds since the Unix epoch,
    /// or one more than the last cursor if that is not greater.
    /// </returns>
    public static long Next(DateTimeOffset timestamp) => 
        Sequence.Next((long)(timestamp - DateTimeOffset.UnixEpoch).TotalMicroseconds);
}