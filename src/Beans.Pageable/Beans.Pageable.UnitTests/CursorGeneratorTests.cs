using Beans.Common.Providers;

namespace Beans.Pageable.UnitTests;

// The generator shares one sequence for the whole process, so these tests only check what holds whatever else has run:
// cursors are never lower than the timestamp, and never repeat or go backwards.
public static class CursorGeneratorTests
{
    private static readonly DateTimeOffset FixedTime = new(2026, 1, 1, 0, 0, 0, TimeSpan.FromHours(5));

    private static long Microseconds(DateTimeOffset timestamp)
        => (long)(timestamp - DateTimeOffset.UnixEpoch).TotalMicroseconds;

    public class Next
    {
        [Fact]
        public void ShouldNotReturnLessThanTheCurrentTime()
        {
            var before = DateTimeOffset.UtcNow;

            var actual = CursorGenerator.Next();

            Assert.True(actual >= Microseconds(before));
        }

        [Fact]
        public void ShouldReturnIncreasingCursors_WhenCalledRepeatedly()
        {
            var cursors = Enumerable.Range(0, 1_000).Select(_ => CursorGenerator.Next()).ToList();

            Assert.Equal(cursors.Order(), cursors);
            Assert.Equal(1_000, cursors.Distinct().Count());
        }

        [Fact]
        public void ShouldReturnDistinctIncreasingCursors_WhenTheProviderTimeIsFixed()
        {
            using var _ = new DateTimeOffsetProviderContext(FixedTime);

            var cursors = Enumerable.Range(0, 1_000).Select(_ => CursorGenerator.Next()).ToList();

            Assert.Equal(cursors.Order(), cursors);
            Assert.Equal(1_000, cursors.Distinct().Count());
        }
    }

    public class NextWithTimestamp
    {
        [Fact]
        public void ShouldBeMicrosecondsSinceTheUnixEpoch()
        {
            var timestamp = DateTimeOffset.UtcNow;

            var actual = CursorGenerator.Next(timestamp);

            Assert.InRange(actual, Microseconds(timestamp), Microseconds(timestamp) + 60_000_000);
        }

        [Fact]
        public void ShouldNotReturnLessThanTheTimestamp()
        {
            var timestamp = DateTimeOffset.UtcNow;

            var actual = CursorGenerator.Next(timestamp);

            Assert.True(actual >= Microseconds(timestamp));
        }

        [Fact]
        public void ShouldUseTheInstant_RegardlessOfTheOffset()
        {
            var utc = DateTimeOffset.UtcNow;
            var sameInstantElsewhere = utc.ToOffset(TimeSpan.FromHours(-7));

            var first = CursorGenerator.Next(utc);
            var second = CursorGenerator.Next(sameInstantElsewhere);

            Assert.True(first >= Microseconds(utc));
            Assert.True(second > first);
        }

        [Fact]
        public void ShouldReturnDistinctCursors_WhenGivenTheSameTimestamp()
        {
            var timestamp = DateTimeOffset.UtcNow;

            var cursors = Enumerable.Range(0, 100).Select(_ => CursorGenerator.Next(timestamp)).ToList();

            Assert.Equal(100, cursors.Distinct().Count());
        }
    }
}