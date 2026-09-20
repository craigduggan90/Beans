using Beans.Requestable.UnitTests.TestSupport;

namespace Beans.Requestable.UnitTests;

public static class RequestableExceptionTests
{
    public class Constructor
    {
        [Fact]
        public void ShouldInitialiseObject_WithMessage()
        {
            var actual = new RequestableException("expected message");

            Assert.Equal("expected message", actual.Message);
            Assert.Null(actual.InnerException);
        }

        [Fact]
        public void ShouldInitialiseObject_WithMessageAndInnerException()
        {
            var innerException = new InvalidOperationException("this is an invalid operation.");

            var actual = new RequestableException("expected message", innerException);

            Assert.Equal("expected message", actual.Message);
            Assert.Same(innerException, actual.InnerException);
        }
    }

    public class ForRequest
    {
        [Fact]
        public void ShouldCreateExceptionWithExpectedMessage()
        {
            var actual = RequestableException.ForRequest(typeof(EchoRequest));

            Assert.Equal("Unable to resolve handler for 'EchoRequest' request.", actual.Message);
        }
    }

    public class ForFailedInstantiation
    {
        [Fact]
        public void ShouldCreateExceptionWithExpectedMessage()
        {
            var actual = RequestableException.ForFailedInstantiation(typeof(EchoRequest));

            Assert.Equal("Unable to instantiate handler for 'EchoRequest' request.", actual.Message);
        }
    }
}