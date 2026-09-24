# Beans.Cacheable

A small `IDistributedCache` wrapper for the get-or-set pattern, because I keep hand-rolling the same
serialize/deserialize boilerplate around it.

[`HybridCache`](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/hybrid) exists and does this
better - with in-process caching, stampede protection and tag-based invalidation. Reach for that first. This
package is for the times you just want `IDistributedCache` with the JSON handled for you, and nothing else.

## Getting Started

`AddCacheableServices` registers `IDistributedCacheClient`. It needs an `IDistributedCache` already registered,
e.g. via `AddDistributedMemoryCache` or a provider-specific extension such as `AddStackExchangeRedisCache`.

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddStackExchangeRedisCache(options => options.Configuration = "localhost:6379");
builder.Services.AddCacheableServices();
```

## Usage

```csharp
public sealed class ProductService(IDistributedCacheClient cache, IProductStore store)
{
    public Task<Product?> GetAsync(Guid id, CancellationToken cancellationToken)
        => cache.GetOrCreateAsync(
            $"product:{id}",
            token => store.FindAsync(id, token),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) },
            cancellationToken);

    public Task RemoveAsync(Guid id, CancellationToken cancellationToken)
        => cache.RemoveAsync($"product:{id}", cancellationToken);
}
```

- **`GetOrCreateAsync`** - returns the cached value for the key, or calls `valueFactory`, caches the result and
  returns it when nothing is cached. A `null` result is cached too, so a factory that legitimately produces `null`
  isn't invoked again on the next call. `options` controls the new entry's expiration; when omitted, the
  underlying cache's own defaults apply.
- **`RemoveAsync`** - removes the cached value for a key. A no-op when the key isn't cached.

This is deliberately simple - concurrent calls for the same missing key can all run `valueFactory`, and there's
no in-process layer in front of the distributed cache. If that matters to you, use `HybridCache` instead.

## Serialization

Values are serialized with System.Text.Json. Register your own `JsonSerializerOptions` singleton with the
container and `DistributedCacheClient` will use it - handy for reusing whatever options your app already
configures for its own JSON, such as options extracted from ASP.NET Core's `JsonOptions`:

```csharp
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<JsonOptions>>().Value.SerializerOptions);
```

When nothing is registered, `JsonSerializerOptions.Web` is used.
