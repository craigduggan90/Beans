namespace Beans.Common.Providers;

/// <summary>DateTime provider allowing fixture in test classes.</summary>
public static class DateTimeOffsetProvider
{
    /// <inheritdoc cref="DateTimeOffset.Now"/>
    public static DateTimeOffset Now
        => DateTimeOffsetProviderContext.Current?.Timestamp ?? DateTimeOffset.Now;

    /// <inheritdoc cref="DateTimeOffset.UtcNow"/>
    public static DateTimeOffset UtcNow
        => DateTimeOffsetProviderContext.Current?.Timestamp.ToUniversalTime() ?? DateTimeOffset.UtcNow;
}