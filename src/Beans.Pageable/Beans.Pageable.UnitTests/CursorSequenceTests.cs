namespace Beans.Pageable.UnitTests;

public static class CursorSequenceTests
{
    public class Next
    {
        [Fact]
        public void ShouldReturnTheValue_WhenItIsTheFirstCursor()
        {
            var sut = new CursorSequence();

            Assert.Equal(1_000, sut.Next(1_000));
        }

        [Fact]
        public void ShouldReturnTheValue_WhenItIsGreaterThanTheLastCursor()
        {
            var sut = new CursorSequence();
            sut.Next(1_000);

            Assert.Equal(5_000, sut.Next(5_000));
        }

        [Fact]
        public void ShouldReturnOneMoreThanTheLastCursor_WhenTheValueIsTheSame()
        {
            var sut = new CursorSequence();
            sut.Next(1_000);

            Assert.Equal(1_001, sut.Next(1_000));
            Assert.Equal(1_002, sut.Next(1_000));
        }

        [Fact]
        public void ShouldReturnOneMoreThanTheLastCursor_WhenTheClockHasGoneBackwards()
        {
            var sut = new CursorSequence();
            sut.Next(5_000);

            Assert.Equal(5_001, sut.Next(1_000));
        }

        [Fact]
        public void ShouldCarryOnFromTheValue_WhenTheClockCatchesUp()
        {
            var sut = new CursorSequence();
            sut.Next(1_000);
            sut.Next(1_000);
            sut.Next(1_000);

            Assert.Equal(2_000, sut.Next(2_000));
        }

        [Fact]
        public void ShouldNeverRepeatACursor_WhenCalledFromManyThreads()
        {
            var sut = new CursorSequence();
            var cursors = new System.Collections.Concurrent.ConcurrentBag<long>();

            Parallel.For(0, 100_000, _ => cursors.Add(sut.Next(1_000)));

            Assert.Equal(100_000, cursors.Distinct().Count());
            Assert.Equal(1_000, cursors.Min());
            Assert.Equal(1_000 + 99_999, cursors.Max());
        }
    }
}