using Beans.Common.Providers;

namespace Beans.Common.UnitTests.Providers;

public static class GuidProviderTests
{
    public class New
    {
        [Fact]
        public void ShouldReturnNewGuid_WhenNoContextConfigured()
        {
            var actual = GuidProvider.New;

            Assert.NotEqual(Guid.Empty, actual);
        }

        [Fact]
        public void ShouldReturnConfiguredGuid_WhenContextConfigured()
        {
            var fixedGuid = new Guid("7e0c1088-c1ef-4eff-8711-5638e9d99f4f");
            using var ambientContext = new GuidProviderContext(fixedGuid);

            var actual = GuidProvider.New;

            Assert.Equal(fixedGuid, actual);
        }

        [Fact]
        public void ShouldOnlyApplyToScope_WhenContextConfiguredInUsingBlock()
        {
            var fixedGuid = new Guid("2d4efedc-dfa7-49fa-8d96-00e486059f71");

            using (var _ = new GuidProviderContext(fixedGuid))
            {
                Assert.Equal(fixedGuid, GuidProvider.New);
            }

            Assert.NotEqual(fixedGuid, GuidProvider.New);
        }

        [Fact]
        public async Task ShouldReturnConfiguredGuid_WhenReadOnAnotherThread()
        {
            var fixedGuid = new Guid("7e0c1088-c1ef-4eff-8711-5638e9d99f4f");
            using var ambientContext = new GuidProviderContext(fixedGuid);

            var actual = await Task.Run(() => GuidProvider.New);

            Assert.Equal(fixedGuid, actual);
        }

        [Fact]
        public void ShouldReturnOuterGuid_WhenInnerContextIsDisposed()
        {
            var outerGuid = new Guid("7e0c1088-c1ef-4eff-8711-5638e9d99f4f");
            var innerGuid = new Guid("2d4efedc-dfa7-49fa-8d96-00e486059f71");
            using var outerContext = new GuidProviderContext(outerGuid);

            using (var _ = new GuidProviderContext(innerGuid))
            {
                Assert.Equal(innerGuid, GuidProvider.New);
            }

            Assert.Equal(outerGuid, GuidProvider.New);
        }

        [Fact]
        public void ShouldReturnNewGuid_WhenContextsAreDisposedOutOfOrder()
        {
            var outerGuid = new Guid("7e0c1088-c1ef-4eff-8711-5638e9d99f4f");
            var innerGuid = new Guid("2d4efedc-dfa7-49fa-8d96-00e486059f71");
            var outerContext = new GuidProviderContext(outerGuid);
            var innerContext = new GuidProviderContext(innerGuid);

            outerContext.Dispose();
            Assert.Equal(innerGuid, GuidProvider.New);

            innerContext.Dispose();
            Assert.NotEqual(outerGuid, GuidProvider.New);
            Assert.NotEqual(innerGuid, GuidProvider.New);
        }

        [Fact]
        public void ShouldKeepActiveContext_WhenAContextIsDisposedTwice()
        {
            var outerGuid = new Guid("7e0c1088-c1ef-4eff-8711-5638e9d99f4f");
            var innerGuid = new Guid("2d4efedc-dfa7-49fa-8d96-00e486059f71");
            var siblingGuid = new Guid("c9a1f1a4-3a53-4f0e-9c7f-7c2e5b0d8a11");
            using var outerContext = new GuidProviderContext(outerGuid);
            var innerContext = new GuidProviderContext(innerGuid);
            innerContext.Dispose();
            using var siblingContext = new GuidProviderContext(siblingGuid);

            innerContext.Dispose();

            Assert.Equal(siblingGuid, GuidProvider.New);
        }

        [Fact]
        public async Task ShouldIsolateContexts_WhenRunConcurrently()
        {
            var first = new Guid("7e0c1088-c1ef-4eff-8711-5638e9d99f4f");
            var second = new Guid("2d4efedc-dfa7-49fa-8d96-00e486059f71");
            var started = 0;
            var bothStarted = new TaskCompletionSource();

            async Task<Guid> ReadWithinContext(Guid value)
            {
                using var ambientContext = new GuidProviderContext(value);

                // wait until both contexts are active so the reads below overlap
                if (Interlocked.Increment(ref started) == 2)
                    bothStarted.SetResult();
                await bothStarted.Task;

                return GuidProvider.New;
            }

            var actual = await Task.WhenAll(
                Task.Run(() => ReadWithinContext(first)),
                Task.Run(() => ReadWithinContext(second)));

            Assert.Equal(first, actual[0]);
            Assert.Equal(second, actual[1]);
        }
    }
}