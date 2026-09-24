using Beans.Cacheable.UnitTests.TestSupport;
using Microsoft.Extensions.Caching.Distributed;
using System.Text;
using System.Text.Json;

namespace Beans.Cacheable.UnitTests;

public static class DistributedCacheClientTests
{
    public class GetOrCreateAsync
    {
        [Fact]
        public async Task ShouldThrowArgumentNullException_WhenKeyIsNull()
        {
            var sut = new DistributedCacheClient(new FakeDistributedCache());

            await Assert.ThrowsAsync<ArgumentNullException>(
                () => sut.GetOrCreateAsync(null!, _ => Task.FromResult("value"), cancellationToken: TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task ShouldThrowArgumentNullException_WhenValueFactoryIsNull()
        {
            var sut = new DistributedCacheClient(new FakeDistributedCache());

            await Assert.ThrowsAsync<ArgumentNullException>(
                () => sut.GetOrCreateAsync<string>("key", null!, cancellationToken: TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task ShouldInvokeValueFactoryAndCacheTheResult_WhenNothingIsCached()
        {
            var cache = new FakeDistributedCache();
            var sut = new DistributedCacheClient(cache);
            var widget = new Widget("gadget", 3);

            var result = await sut.GetOrCreateAsync("key", _ => Task.FromResult(widget), cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equal(widget, result);
            var (key, value, _) = Assert.Single(cache.SetCalls);
            Assert.Equal("key", key);
            Assert.Equal(widget, JsonSerializer.Deserialize<Widget>(value, JsonSerializerOptions.Web));
        }

        [Fact]
        public async Task ShouldReturnTheCachedValueWithoutInvokingTheValueFactory_WhenAValueIsCached()
        {
            var cache = new FakeDistributedCache();
            var widget = new Widget("gadget", 3);
            cache.Set("key", JsonSerializer.SerializeToUtf8Bytes(widget, JsonSerializerOptions.Web), new());
            var sut = new DistributedCacheClient(cache);
            var factoryCalled = false;

            var result = await sut.GetOrCreateAsync(
                "key",
                _ =>
                {
                    factoryCalled = true;
                    return Task.FromResult(new Widget("other", 0));
                },
                cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equal(widget, result);
            Assert.False(factoryCalled);
            Assert.Empty(cache.SetCalls);
        }

        [Fact]
        public async Task ShouldCacheAndReturnNull_WhenTheValueFactoryProducesNull()
        {
            var cache = new FakeDistributedCache();
            var sut = new DistributedCacheClient(cache);

            var result = await sut.GetOrCreateAsync<Widget?>(
                "key", _ => Task.FromResult<Widget?>(null), cancellationToken: TestContext.Current.CancellationToken);

            Assert.Null(result);
            Assert.Single(cache.SetCalls);
        }

        [Fact]
        public async Task ShouldNotInvokeTheValueFactory_WhenACachedNullIsFound()
        {
            var cache = new FakeDistributedCache();
            cache.Set("key", JsonSerializer.SerializeToUtf8Bytes<Widget?>(null, JsonSerializerOptions.Web), new());
            var sut = new DistributedCacheClient(cache);
            var factoryCalled = false;

            var result = await sut.GetOrCreateAsync<Widget?>(
                "key",
                _ =>
                {
                    factoryCalled = true;
                    return Task.FromResult<Widget?>(new Widget("other", 0));
                },
                cancellationToken: TestContext.Current.CancellationToken);

            Assert.Null(result);
            Assert.False(factoryCalled);
        }

        [Fact]
        public async Task ShouldPassTheCancellationTokenToTheValueFactory()
        {
            var cache = new FakeDistributedCache();
            var sut = new DistributedCacheClient(cache);
            using var cts = new CancellationTokenSource();
            CancellationToken? received = null;

            await sut.GetOrCreateAsync(
                "key",
                token =>
                {
                    received = token;
                    return Task.FromResult("value");
                },
                cancellationToken: cts.Token);

            Assert.Equal(cts.Token, received);
        }

        [Fact]
        public async Task ShouldUseTheSuppliedEntryOptions_WhenCachingANewValue()
        {
            var cache = new FakeDistributedCache();
            var sut = new DistributedCacheClient(cache);
            var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) };

            await sut.GetOrCreateAsync("key", _ => Task.FromResult("value"), options, TestContext.Current.CancellationToken);

            var (_, _, usedOptions) = Assert.Single(cache.SetCalls);
            Assert.Same(options, usedOptions);
        }

        [Fact]
        public async Task ShouldUseTheRegisteredJsonSerializerOptions_WhenOneIsSupplied()
        {
            var cache = new FakeDistributedCache();
            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };
            var sut = new DistributedCacheClient(cache, jsonOptions);

            await sut.GetOrCreateAsync("key", _ => Task.FromResult(new Widget("gadget", 3)), cancellationToken: TestContext.Current.CancellationToken);

            var (_, value, _) = Assert.Single(cache.SetCalls);
            Assert.Contains("\"count\"", Encoding.UTF8.GetString(value));
        }

        [Fact]
        public async Task ShouldFallBackToWebDefaults_WhenNoJsonSerializerOptionsAreSupplied()
        {
            var cache = new FakeDistributedCache();
            var sut = new DistributedCacheClient(cache);

            await sut.GetOrCreateAsync("key", _ => Task.FromResult(new Widget("gadget", 3)), cancellationToken: TestContext.Current.CancellationToken);

            var (_, value, _) = Assert.Single(cache.SetCalls);
            Assert.Contains("\"name\"", Encoding.UTF8.GetString(value));
        }
    }

    public class RemoveAsync
    {
        [Fact]
        public async Task ShouldThrowArgumentNullException_WhenKeyIsNull()
        {
            var sut = new DistributedCacheClient(new FakeDistributedCache());

            await Assert.ThrowsAsync<ArgumentNullException>(
                () => sut.RemoveAsync(null!, TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task ShouldRemoveTheCachedValue()
        {
            var cache = new FakeDistributedCache();
            cache.Set("key", "value"u8.ToArray(), new());
            var sut = new DistributedCacheClient(cache);

            await sut.RemoveAsync("key", TestContext.Current.CancellationToken);

            Assert.Null(cache.Get("key"));
        }

        [Fact]
        public async Task ShouldBeANoOp_WhenTheKeyIsNotCached()
        {
            var cache = new FakeDistributedCache();
            var sut = new DistributedCacheClient(cache);

            await sut.RemoveAsync("key", TestContext.Current.CancellationToken);

            Assert.Equal("key", Assert.Single(cache.RemoveCalls));
        }
    }
}