using Beans.Common.Services;

namespace Beans.Common.UnitTests.Services;

public static class EnvironmentVariableAccessorTests
{
    // Each nested class is its own xunit test class, and those run in parallel, so each one
    // works with a differently named variable rather than sharing process-wide state.

    public class Get
    {
        private const string Variable = "beans-common-test-get-variable";

        [Fact]
        public void ShouldReturnValue_WhenValueIsAvailable()
        {
            Environment.SetEnvironmentVariable(Variable, "initial-value");
            var sut = CreateSut();

            var result = sut.Get(Variable);

            Assert.Equal("initial-value", result);
        }

        [Fact]
        public void ShouldReturnNull_WhenValueIsUnavailable()
        {
            Environment.SetEnvironmentVariable(Variable, null);
            var sut = CreateSut();

            var result = sut.Get(Variable);

            Assert.Null(result);
        }
    }

    public class Set
    {
        private const string Variable = "beans-common-test-set-variable";

        [Fact]
        public void ShouldUpdateValue_WhenValueIsProvided()
        {
            Environment.SetEnvironmentVariable(Variable, "initial-value");
            var sut = CreateSut();

            sut.Set(Variable, "new-value");

            Assert.Equal("new-value", Environment.GetEnvironmentVariable(Variable));
        }

        [Fact]
        public void ShouldRemoveVariable_WhenValueIsNull()
        {
            Environment.SetEnvironmentVariable(Variable, "initial-value");
            var sut = CreateSut();

            sut.Set(Variable, null);

            Assert.Null(Environment.GetEnvironmentVariable(Variable));
        }
    }

    private static EnvironmentVariableAccessor CreateSut() => new();
}