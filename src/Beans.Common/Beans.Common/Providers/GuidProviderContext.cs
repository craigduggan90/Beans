using System.Diagnostics.CodeAnalysis;

namespace Beans.Common.Providers;

/// <summary>Execution context for the <see cref="GuidProvider"/> class</summary>
/// <remarks>This is excluded from code coverage as it's behaviour is tested through <see cref="GuidProvider" /></remarks>
[ExcludeFromCodeCoverage]
public class GuidProviderContext : IDisposable
{
    internal Guid Value;

    // Each context points at the one that was current when it was created, rather than sharing a mutable stack.
    // AsyncLocal values are copied into child flows, so anything shared and mutable would leak between them.
    private static readonly AsyncLocal<GuidProviderContext?> CurrentContext = new();
    private readonly GuidProviderContext? _parent;
    private bool _disposed;

    /// <summary>Initializes a new instance of the <see cref="GuidProviderContext"/> class.</summary>
    /// <param name="value"></param>
    public GuidProviderContext(Guid value)
    {
        Value = value;
        _parent = CurrentContext.Value;
        CurrentContext.Value = this;
    }

    /// <summary>
    /// The guid configured for the current execution context
    /// </summary>
    public static GuidProviderContext? Current
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