using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Beans.Cacheable;

/// <inheritdoc/>
/// <remarks>
/// <para>
/// Values are serialized with System.Text.Json. <paramref name="jsonSerializerOptions"/> is an optional DI
/// dependency: register your own <see cref="JsonSerializerOptions"/> singleton with the container (for example,
/// the same instance your app configures for its API responses) and it is picked up automatically. When nothing
/// is registered, <see cref="JsonSerializerOptions.Web"/> is used instead.
/// </para>
/// </remarks>
public class DistributedCacheClient(IDistributedCache cache, JsonSerializerOptions? jsonSerializerOptions = null)
    : IDistributedCacheClient
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = jsonSerializerOptions ?? JsonSerializerOptions.Web;

    /// <inheritdoc/>
    public async Task<T?> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> valueFactory,
        DistributedCacheEntryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(valueFactory);

        var cached = await cache.GetAsync(key, cancellationToken).ConfigureAwait(false);
        if (cached is not null)
            return JsonSerializer.Deserialize<T>(cached, _jsonSerializerOptions);

        var value = await valueFactory(cancellationToken).ConfigureAwait(false);

        var serialized = JsonSerializer.SerializeToUtf8Bytes(value, _jsonSerializerOptions);
        await cache.SetAsync(key, serialized, options ?? new DistributedCacheEntryOptions(), cancellationToken)
            .ConfigureAwait(false);

        return value;
    }

    /// <inheritdoc/>
    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);
        return cache.RemoveAsync(key, cancellationToken);
    }
}