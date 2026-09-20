namespace Beans.Pageable.UnitTests;

public static class CursorExtensionsTests
{
    public class EncodeCursor
    {
        [Fact]
        public void ShouldReturnNull_WhenInputIsNull()
        {
            var actual = ((long?)null).EncodeCursor();

            Assert.Null(actual);
        }

        [Theory]
        [InlineData(0L, "MA==")]
        [InlineData(123L, "MTIz")]
        [InlineData(-5L, "LTU=")]
        [InlineData(long.MaxValue, "OTIyMzM3MjAzNjg1NDc3NTgwNw==")]
        public void ShouldReturnBase64OfTheNumber_WhenInputHasAValue(long input, string expected)
        {
            var actual = ((long?)input).EncodeCursor();

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(0L, "MA==")]
        [InlineData(123L, "MTIz")]
        [InlineData(-5L, "LTU=")]
        [InlineData(long.MaxValue, "OTIyMzM3MjAzNjg1NDc3NTgwNw==")]
        public void ShouldReturnBase64OfTheNumber_WhenInputIsNotNullable(long input, string expected)
        {
            var actual = input.EncodeCursor();

            Assert.Equal(expected, actual);
        }
    }

    public class TryDecodeCursor
    {
        [Fact]
        public void ShouldReturnTrueAndNullCursor_WhenInputIsNull()
        {
            var success = ((string?)null).TryDecodeCursor(out var cursor);

            Assert.True(success);
            Assert.Null(cursor);
        }

        [Theory]
        [InlineData("MA==", 0L)]
        [InlineData("MTIz", 123L)]
        [InlineData("LTU=", -5L)]
        public void ShouldReturnTheNumber_WhenInputIsAValidCursor(string input, long expected)
        {
            var success = input.TryDecodeCursor(out var cursor);

            Assert.True(success);
            Assert.Equal(expected, cursor);
        }

        [Theory]
        [InlineData("")]
        [InlineData("not base64!")]
        [InlineData("MTIz=")]
        public void ShouldReturnFalseAndNullCursor_WhenInputIsNotValidBase64(string input)
        {
            var success = input.TryDecodeCursor(out var cursor);

            Assert.False(success);
            Assert.Null(cursor);
        }

        [Theory]
        [InlineData("YWJj")]
        [InlineData("MS41")]
        [InlineData("OTIyMzM3MjAzNjg1NDc3NTgwOA==")]
        public void ShouldReturnFalseAndNullCursor_WhenInputDecodesToSomethingThatIsNotALong(string input)
        {
            var success = input.TryDecodeCursor(out var cursor);

            Assert.False(success);
            Assert.Null(cursor);
        }

        [Theory]
        [InlineData(0L)]
        [InlineData(1L)]
        [InlineData(-1L)]
        [InlineData(long.MinValue)]
        [InlineData(long.MaxValue)]
        public void ShouldReturnTheOriginalNumber_WhenGivenTheResultOfEncoding(long original)
        {
            var encoded = original.EncodeCursor();

            var success = encoded.TryDecodeCursor(out var decoded);

            Assert.True(success);
            Assert.Equal(original, decoded);
        }
    }
}