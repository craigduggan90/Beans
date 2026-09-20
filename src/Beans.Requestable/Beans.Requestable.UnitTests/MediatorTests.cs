using Beans.Requestable.UnitTests.TestSupport;
using Microsoft.Extensions.DependencyInjection;

namespace Beans.Requestable.UnitTests;

public static class MediatorTests
{
    private static Mediator CreateSut(Action<IServiceCollection>? register = null)
    {
        var services = new ServiceCollection();
        register?.Invoke(services);
        return new Mediator(services.BuildServiceProvider());
    }

    public class SendAsyncWithoutResponse
    {
        [Fact]
        public async Task ShouldThrowException_WhenHandlerNotRegistered()
        {
            var sut = CreateSut();

            var exception = await Assert.ThrowsAsync<RequestHandlerException>(
                () => sut.SendAsync(new UnhandledVoidRequest(), TestContext.Current.CancellationToken));

            Assert.Equal("Unable to resolve handler for 'UnhandledVoidRequest' request.", exception.Message);
        }

        [Fact]
        public async Task ShouldInvokeHandlerWithRequestAndCancellationToken_WhenHandlerRegistered()
        {
            var handler = new PingRequestHandler();
            var sut = CreateSut(s => s.AddSingleton<IRequestHandler<PingRequest>>(handler));
            var request = new PingRequest();
            using var cts = new CancellationTokenSource();

            await sut.SendAsync(request, cts.Token);

            Assert.Same(request, Assert.Single(handler.Received));
            Assert.Equal(cts.Token, handler.ReceivedToken);
        }

        [Fact]
        public async Task ShouldInvokeHandlerEveryTime_WhenSentRepeatedly()
        {
            var handler = new PingRequestHandler();
            var sut = CreateSut(s => s.AddSingleton<IRequestHandler<PingRequest>>(handler));

            await sut.SendAsync(new PingRequest(), TestContext.Current.CancellationToken);
            await sut.SendAsync(new PingRequest(), TestContext.Current.CancellationToken);

            Assert.Equal(2, handler.Received.Count);
        }

        [Fact]
        public async Task ShouldThrowArgumentNullException_WhenRequestIsNull()
        {
            var sut = CreateSut();

            await Assert.ThrowsAsync<ArgumentNullException>(
                () => sut.SendAsync((IRequest)null!, TestContext.Current.CancellationToken));
        }
    }

    public class SendAsyncWithResponse
    {
        [Fact]
        public async Task ShouldThrowException_WhenHandlerNotRegistered()
        {
            var sut = CreateSut();

            var exception = await Assert.ThrowsAsync<RequestHandlerException>(
                () => sut.SendAsync(new UnhandledRequest(), TestContext.Current.CancellationToken));

            Assert.Equal("Unable to resolve handler for 'UnhandledRequest' request.", exception.Message);
        }

        [Fact]
        public async Task ShouldReturnHandlerResponse_WhenHandlerRegistered()
        {
            var sut = CreateSut(s => s.AddTransient<IRequestHandler<EchoRequest, string>, EchoRequestHandler>());

            var actual = await sut.SendAsync(new EchoRequest("hello"), TestContext.Current.CancellationToken);

            Assert.Equal("hello", actual);
        }

        [Fact]
        public async Task ShouldPassCancellationTokenToHandler()
        {
            var handler = new EchoRequestHandler();
            var sut = CreateSut(s => s.AddSingleton<IRequestHandler<EchoRequest, string>>(handler));
            using var cts = new CancellationTokenSource();

            await sut.SendAsync(new EchoRequest("hello"), cts.Token);

            Assert.Equal(cts.Token, handler.ReceivedToken);
        }

        [Fact]
        public async Task ShouldPropagateException_WhenHandlerThrows()
        {
            var sut = CreateSut(
                s => s.AddTransient<IRequestHandler<ThrowingRequest, string>, ThrowingRequestHandler>());

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => sut.SendAsync(new ThrowingRequest(), TestContext.Current.CancellationToken));

            Assert.Equal("handler failed", exception.Message);
        }

        [Fact]
        public async Task ShouldResolveHandlerPerSend_WhenSentRepeatedly()
        {
            var sut = CreateSut(s => s.AddTransient<IRequestHandler<EchoRequest, string>, EchoRequestHandler>());

            var first = await sut.SendAsync(new EchoRequest("first"), TestContext.Current.CancellationToken);
            var second = await sut.SendAsync(new EchoRequest("second"), TestContext.Current.CancellationToken);

            Assert.Equal("first", first);
            Assert.Equal("second", second);
        }

        [Fact]
        public async Task ShouldRouteByResponseType_WhenRequestHasMoreThanOneResponse()
        {
            var sut = CreateSut(s => s
                .AddTransient<IRequestHandler<DualRequest, int>, DualIntRequestHandler>()
                .AddTransient<IRequestHandler<DualRequest, string>, DualStringRequestHandler>());
            var request = new DualRequest();

            var number = await sut.SendAsync<int>(request, TestContext.Current.CancellationToken);
            var text = await sut.SendAsync<string>(request, TestContext.Current.CancellationToken);

            Assert.Equal(42, number);
            Assert.Equal("forty-two", text);
        }

        [Fact]
        public async Task ShouldThrowArgumentNullException_WhenRequestIsNull()
        {
            var sut = CreateSut();

            await Assert.ThrowsAsync<ArgumentNullException>(
                () => sut.SendAsync<string>(null!, TestContext.Current.CancellationToken));
        }
    }
}