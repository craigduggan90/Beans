using Microsoft.Extensions.Caching.Distributed;

namespace Beans.Cacheable;

/// <summary>A JSON-serializing wrapper over <see cref="IDistributedCache"/>.</summary>
public interface IDistributedCacheClient
{
    /// <summary>Returns the cached value for <paramref name="key"/>, caching it when it is missing.</summary>
    /// <typeparam name="T">The type of the cached value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="valueFactory">Produces the value when nothing is cached for <paramref name="key"/>.</param>
    /// <param name="options">
    /// The entry options to use when caching a newly produced value, e.g. its expiration.  When not supplied, the
    /// cache's own defaults apply.
    /// </param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The cached value, or the value produced by <paramref name="valueFactory"/>.</returns>
    Task<T?> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> valueFactory,
        DistributedCacheEntryOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Removes the cached value for <paramref name="key"/>, if one exists.</summary>
    /// <param name="key">The cache key.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that completes once the key has been removed.  A no-op when the key is not cached.</returns>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}