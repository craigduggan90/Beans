using Microsoft.Extensions.Caching.Distributed;

namespace Beans.Cacheable.UnitTests.TestSupport;

/// <summary>An in-memory <see cref="IDistributedCache"/> that records what it was asked to do.</summary>
internal sealed class FakeDistributedCache : IDistributedCache
{
    private readonly Dictionary<string, byte[]> _entries = [];

    public List<string> GetCalls { get; } = [];

    public List<(string Key, byte[] Value, DistributedCacheEntryOptions Options)> SetCalls { get; } = [];

    public List<string> RemoveCalls { get; } = [];

    public byte[]? Get(string key) => _entries.GetValueOrDefault(key);

    public Task<byte[]?> GetAsync(string key, CancellationToken token = default)
    {
        GetCalls.Add(key);
        return Task.FromResult(_entries.GetValueOrDefault(key));
    }

    public void Set(string key, byte[] value, DistributedCacheEntryOptions options) => _entries[key] = value;

    public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
    {
        SetCalls.Add((key, value, options));
        _entries[key] = value;
        return Task.CompletedTask;
    }

    public void Refresh(string key)
    {
    }

    public Task RefreshAsync(string key, CancellationToken token = default) => Task.CompletedTask;

    public void Remove(string key) => _entries.Remove(key);

    public Task RemoveAsync(string key, CancellationToken token = default)
    {
        RemoveCalls.Add(key);
        _entries.Remove(key);
        return Task.CompletedTask;
    }
}