namespace Beans.Patchable.UnitTests;

public static class OptionalTests
{
    public class Unset
    {
        [Fact]
        public void ShouldNotBeSet()
        {
            var optional = Optional<string>.Unset;

            Assert.False(optional.IsSet);
        }

        [Fact]
        public void ShouldReturnFalseAndDefaultValue_WhenValueIsRetrieved()
        {
            var optional = Optional<string>.Unset;

            var result = optional.TryGetValue(out var value);

            Assert.False(result);
            Assert.Null(value);
        }

        [Fact]
        public void ShouldReturnDefault_WhenValueIsAccessed()
        {
            var optional = Optional<string>.Unset;

            Assert.Null(optional.Value);
        }

        [Fact]
        public void ShouldExposeTheGenericType()
        {
            var optional = Optional<string>.Unset;

            Assert.Equal(typeof(string), optional.ValueType);
        }
    }

    public class Of
    {
        [Fact]
        public void ShouldBeSet_WhenValueIsProvided()
        {
            var optional = Optional<string>.Of("value");

            Assert.True(optional.IsSet);
        }

        [Fact]
        public void ShouldReturnTheValue_WhenValueIsProvided()
        {
            var optional = Optional<string>.Of("value");

            Assert.Equal("value", optional.Value);
        }

        [Fact]
        public void ShouldReturnTrueAndTheValue_WhenValueIsRetrieved()
        {
            var optional = Optional<string>.Of("value");

            var result = optional.TryGetValue(out var value);

            Assert.True(result);
            Assert.Equal("value", value);
        }

        [Fact]
        public void ShouldBeSetAndReturnNull_WhenNullIsProvided()
        {
            var optional = Optional<string?>.Of(null);

            Assert.True(optional.IsSet);
            Assert.Null(optional.Value);
        }

        [Fact]
        public void ShouldReturnTrueAndNull_WhenNullIsRetrieved()
        {
            var optional = Optional<string?>.Of(null);

            var result = optional.TryGetValue(out var value);

            Assert.True(result);
            Assert.Null(value);
        }

        [Fact]
        public void ShouldExposeTheGenericType()
        {
            var optional = Optional<int>.Of(42);

            Assert.Equal(typeof(int), optional.ValueType);
        }
    }
}