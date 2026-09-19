namespace Beans.Common.Providers;

/// <summary>Guid provider allowing fixture in test classes.</summary>
public static class GuidProvider
{
    /// <inheritdoc cref="Guid.NewGuid"/>
    public static Guid New => GuidProviderContext.Current?.Value ?? Guid.NewGuid();
}