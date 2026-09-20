namespace Beans.Pageable;

/// <summary>Generates out cursors that only ever increase within the process.</summary>
internal sealed class CursorSequence
{
    private long _last;

    /// <summary>Gets the next cursor.</summary>
    /// <param name="value">The value (a timestamp in microseconds) that the cursor should be based on.</param>
    /// <returns>
    /// <paramref name="value"/>, unless that is not greater than the last cursor handed out (because entities were
    /// created within the same microsecond, or the clock went backwards), in which case one more than the last cursor.
    /// </returns>
    public long Next(long value)
    {
        // The last cursor handed out. Another thread migth get in there first it on before we can, which the loop below deals with.
        var last = Volatile.Read(ref _last);

        // Retry until we claim a cursor without another thread getting in first.
        while (true)
        {
            // Use value if it's ahead of the last cursor, otherwise there'd be a sequence issue or conflict so we
            // increment the last cursor instead.
            var next = Math.Max(value, last + 1);

            // if _last still equals last, store next in it.  Either way we get back what _last is now.
            var seen = Interlocked.CompareExchange(ref _last, next, last);
            if (seen == last)
                return next;

            // Another thread got there first. Carry on from the value it stored, and work out next again.
            last = seen;
        }
    }
}