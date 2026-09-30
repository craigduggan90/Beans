using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace Beans.Cacheable.IntegrationTests;

/// <summary>
/// Exercises <see cref="IDistributedCacheClient"/> resolved from a real DI container over a real
/// <see cref="IDistributedCache"/>, rather than a fake.
/// </summary>
public sealed class DistributedCacheClientTests
{
    private sealed record Widget(string Name, int Count);

    private static IDistributedCacheClient BuildClient(Action<IServiceCollection>? configure = null)
    {
        var services = new ServiceCollection();
        services.AddDistributedMemoryCache();
        configure?.Invoke(services);
        services.AddCacheableServices();
        return services.BuildServiceProvider().GetRequiredService<IDistributedCacheClient>();
    }

    [Fact]
    public async Task ShouldCacheAcrossCallsThroughARealDistributedCache()
    {
        var sut = BuildClient();
        var calls = 0;

        Task<Widget> Factory(CancellationToken _)
        {
            calls++;
            return Task.FromResult(new Widget("gadget", calls));
        }

        var first = await sut.GetOrCreateAsync("widget", Factory, cancellationToken: TestContext.Current.CancellationToken);
        var second = await sut.GetOrCreateAsync("widget", Factory, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(1, calls);
        Assert.Equal(first, second);
    }

    [Fact]
    public async Task ShouldRecomputeAfterInvalidation()
    {
        var sut = BuildClient();
        var calls = 0;

        Task<Widget> Factory(CancellationToken _)
        {
            calls++;
            return Task.FromResult(new Widget("gadget", calls));
        }

        await sut.GetOrCreateAsync("widget", Factory, cancellationToken: TestContext.Current.CancellationToken);
        await sut.RemoveAsync("widget", TestContext.Current.CancellationToken);
        var afterInvalidation = await sut.GetOrCreateAsync("widget", Factory, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(2, calls);
        Assert.Equal(2, afterInvalidation!.Count);
    }

    [Fact]
    public async Task ShouldUseAJsonSerializerOptionsRegisteredInTheContainer()
    {
        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };
        var cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));

        var sut = BuildClient(services =>
        {
            services.AddSingleton(jsonOptions);
            services.AddSingleton<IDistributedCache>(cache);
        });

        await sut.GetOrCreateAsync("widget", _ => Task.FromResult(new Widget("gadget", 1)), cancellationToken: TestContext.Current.CancellationToken);

        var raw = await cache.GetAsync("widget", TestContext.Current.CancellationToken);
        Assert.Contains("\"count\":1", Encoding.UTF8.GetString(raw!));
    }
}