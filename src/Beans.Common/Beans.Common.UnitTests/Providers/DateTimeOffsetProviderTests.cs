using Beans.Common.Providers;

namespace Beans.Common.UnitTests.Providers;

public static class DateTimeOffsetProviderTests
{
    public class Now
    {
        [Fact]
        public void ShouldReturnCurrentTime_WhenNoContextConfigured()
        {
            var actual = DateTimeOffsetProvider.Now;

            Assert.True((actual - DateTimeOffset.UtcNow).TotalMilliseconds <= 3);
        }

        [Fact]
        public void ShouldReturnConfiguredTime_WhenContextConfigured()
        {
            var fixedTimestamp = new DateTimeOffset(2016, 4, 16, 7, 53, 14, TimeSpan.FromHours(-5));
            using var ambientContext = new DateTimeOffsetProviderContext(fixedTimestamp);

            var actual = DateTimeOffsetProvider.Now;

            Assert.Equal(fixedTimestamp, actual);
        }

        [Fact]
        public void ShouldOnlyApplyToScope_WhenContextConfiguredInUsingBlock()
        {
            var fixedTimestamp = new DateTimeOffset(2016, 4, 16, 7, 53, 14, TimeSpan.FromHours(-5));

            using (var _ = new DateTimeOffsetProviderContext(fixedTimestamp))
            {
                Assert.Equal(fixedTimestamp, DateTimeOffsetProvider.Now);
            }

            Assert.True((DateTimeOffsetProvider.Now - DateTimeOffset.UtcNow).TotalMilliseconds <= 3);
        }

        [Fact]
        public async Task ShouldReturnConfiguredTime_WhenReadOnAnotherThread()
        {
            var fixedTimestamp = new DateTimeOffset(2016, 4, 16, 7, 53, 14, TimeSpan.FromHours(-5));
            using var ambientContext = new DateTimeOffsetProviderContext(fixedTimestamp);

            var actual = await Task.Run(() => DateTimeOffsetProvider.Now);

            Assert.Equal(fixedTimestamp, actual);
        }

        [Fact]
        public void ShouldReturnOuterTime_WhenInnerContextIsDisposed()
        {
            var outerTimestamp = new DateTimeOffset(2016, 4, 16, 7, 53, 14, TimeSpan.FromHours(-5));
            var innerTimestamp = outerTimestamp.AddDays(1);
            using var outerContext = new DateTimeOffsetProviderContext(outerTimestamp);

            using (var _ = new DateTimeOffsetProviderContext(innerTimestamp))
            {
                Assert.Equal(innerTimestamp, DateTimeOffsetProvider.Now);
            }

            Assert.Equal(outerTimestamp, DateTimeOffsetProvider.Now);
        }

        [Fact]
        public void ShouldReturnCurrentTime_WhenContextsAreDisposedOutOfOrder()
        {
            var outerTimestamp = new DateTimeOffset(2016, 4, 16, 7, 53, 14, TimeSpan.FromHours(-5));
            var innerTimestamp = outerTimestamp.AddDays(1);
            var outerContext = new DateTimeOffsetProviderContext(outerTimestamp);
            var innerContext = new DateTimeOffsetProviderContext(innerTimestamp);

            outerContext.Dispose();
            Assert.Equal(innerTimestamp, DateTimeOffsetProvider.Now);

            innerContext.Dispose();
            Assert.True((DateTimeOffsetProvider.Now - DateTimeOffset.UtcNow).TotalMilliseconds <= 3);
        }

        [Fact]
        public void ShouldKeepActiveContext_WhenAContextIsDisposedTwice()
        {
            var outerTimestamp = new DateTimeOffset(2016, 4, 16, 7, 53, 14, TimeSpan.FromHours(-5));
            var innerTimestamp = outerTimestamp.AddDays(1);
            var siblingTimestamp = outerTimestamp.AddDays(2);
            using var outerContext = new DateTimeOffsetProviderContext(outerTimestamp);
            var innerContext = new DateTimeOffsetProviderContext(innerTimestamp);
            innerContext.Dispose();
            using var siblingContext = new DateTimeOffsetProviderContext(siblingTimestamp);

            innerContext.Dispose();

            Assert.Equal(siblingTimestamp, DateTimeOffsetProvider.Now);
        }

        [Fact]
        public async Task ShouldIsolateContexts_WhenRunConcurrently()
        {
            var first = new DateTimeOffset(2016, 4, 16, 7, 53, 14, TimeSpan.FromHours(-5));
            var second = first.AddDays(1);
            var started = 0;
            var bothStarted = new TaskCompletionSource();

            async Task<DateTimeOffset> ReadWithinContext(DateTimeOffset timestamp)
            {
                using var ambientContext = new DateTimeOffsetProviderContext(timestamp);

                // wait until both contexts are active so the reads below overlap
                if (Interlocked.Increment(ref started) == 2)
                    bothStarted.SetResult();
                await bothStarted.Task;

                return DateTimeOffsetProvider.Now;
            }

            var actual = await Task.WhenAll(
                Task.Run(() => ReadWithinContext(first)),
                Task.Run(() => ReadWithinContext(second)));

            Assert.Equal(first, actual[0]);
            Assert.Equal(second, actual[1]);
        }
    }

    public class UtcNow
    {
        [Fact]
        public void ShouldReturnCurrentUtcTime_WhenNoContextConfigured()
        {
            var actual = DateTimeOffsetProvider.UtcNow;

            Assert.Equal(TimeSpan.Zero, actual.Offset);
            Assert.True((actual - DateTimeOffset.UtcNow).TotalMilliseconds <= 3);
        }

        [Fact]
        public void ShouldReturnConfiguredTimeAsUtc_WhenContextConfigured()
        {
            var fixedTimestamp = new DateTimeOffset(2016, 4, 16, 7, 53, 14, TimeSpan.FromHours(-5));
            using var ambientContext = new DateTimeOffsetProviderContext(fixedTimestamp);

            var actual = DateTimeOffsetProvider.UtcNow;

            Assert.Equal(fixedTimestamp, actual);
            Assert.Equal(TimeSpan.Zero, actual.Offset);
        }
    }
}