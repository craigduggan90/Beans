using System.Diagnostics.CodeAnalysis;

namespace Beans.Common.Providers;

/// <summary>Execution context for the <see cref="DateTimeOffsetProvider"/> class</summary>
/// <remarks>This is excluded from code coverage as it's behaviour is tested through <see cref="DateTimeOffsetProvider" /></remarks>
[ExcludeFromCodeCoverage]
public class DateTimeOffsetProviderContext : IDisposable
{
    internal DateTimeOffset Timestamp;

    // Each context points at the one that was current when it was created, rather than sharing a mutable stack.
    // AsyncLocal values are copied into child flows, so anything shared and mutable would leak between them.
    private static readonly AsyncLocal<DateTimeOffsetProviderContext?> CurrentContext = new();
    private readonly DateTimeOffsetProviderContext? _parent;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="DateTimeOffsetProviderContext"/> class
    /// </summary>
    /// <param name="timestamp"></param>
    public DateTimeOffsetProviderContext(DateTimeOffset timestamp)
    {
        Timestamp = timestamp;
        _parent = CurrentContext.Value;
        CurrentContext.Value = this;
    }

    /// <summary>
    /// The timestamp configured for the current execution context
    /// </summary>
    public static DateTimeOffsetProviderContext? Current
    {
        get
        {
            // a disposed context can still be reachable (disposed out of order, or in a different flow), so skip it
            var context = CurrentContext.Value;
            while (context is { _disposed: true })
                context = context._parent;

            return context;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        if (CurrentContext.Value == this)
            CurrentContext.Value = _parent;

        GC.SuppressFinalize(this);
    }
}