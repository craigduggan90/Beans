using Beans.Requestable.UnitTests.TestSupport;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Beans.Requestable.UnitTests;

public static class DependencyInjectionTests
{
    public class AddRequestableServices
    {
        [Fact]
        public void ShouldReturnTheSameServiceCollection()
        {
            var services = new ServiceCollection();

            var result = services.AddRequestableServices();

            Assert.Same(services, result);
        }

        [Fact]
        public void ShouldRegisterMediatorAsTransient()
        {
            var services = new ServiceCollection();

            services.AddRequestableServices();

            var descriptor = Assert.Single(services, d => d.ServiceType == typeof(IMediator));
            Assert.Equal(typeof(Mediator), descriptor.ImplementationType);
            Assert.Equal(ServiceLifetime.Transient, descriptor.Lifetime);
        }

        [Fact]
        public void ShouldRegisterHandlersFromCallingAssembly()
        {
            var services = new ServiceCollection();

            services.AddRequestableServices();

            AssertTransientHandler<IRequestHandler<EchoRequest, string>, EchoRequestHandler>(services);
            AssertTransientHandler<IRequestHandler<PingRequest>, PingRequestHandler>(services);
        }

        [Fact]
        public async Task ShouldResolveMediatorThatCanSendRequests()
        {
            var services = new ServiceCollection();
            services.AddRequestableServices();
            await using var provider = services.BuildServiceProvider();

            var mediator = provider.GetRequiredService<IMediator>();
            var actual = await mediator.SendAsync(new EchoRequest("hello"), TestContext.Current.CancellationToken);

            Assert.Equal("hello", actual);
        }
    }

    public class AddRequestableServicesWithConfiguration
    {
        [Fact]
        public void ShouldReturnTheSameServiceCollection()
        {
            var services = new ServiceCollection();

            var result = services.AddRequestableServices(_ => { });

            Assert.Same(services, result);
        }

        [Fact]
        public void ShouldRegisterHandlersFromCallingAssembly_WhenAssembliesNotSet()
        {
            var services = new ServiceCollection();

            services.AddRequestableServices(_ => { });

            AssertTransientHandler<IRequestHandler<EchoRequest, string>, EchoRequestHandler>(services);
        }

        [Fact]
        public void ShouldRegisterNoHandlers_WhenAssembliesIsEmpty()
        {
            var services = new ServiceCollection();

            services.AddRequestableServices(config => config.Assemblies = []);

            Assert.Single(services);
            Assert.Single(services, d => d.ServiceType == typeof(IMediator));
        }

        [Fact]
        public void ShouldRegisterHandlersFromEachConfiguredAssembly()
        {
            var services = new ServiceCollection();

            services.AddRequestableServices(config => config.Assemblies = [typeof(EchoRequestHandler).Assembly]);

            AssertTransientHandler<IRequestHandler<EchoRequest, string>, EchoRequestHandler>(services);
        }

        [Fact]
        public void ShouldRegisterHandlersOnce_WhenAssemblyIsListedMoreThanOnce()
        {
            var services = new ServiceCollection();
            var assembly = Assembly.GetExecutingAssembly();

            services.AddRequestableServices(config => config.Assemblies = [assembly, assembly]);

            Assert.Single(services, d => d.ServiceType == typeof(IRequestHandler<EchoRequest, string>));
        }

        [Fact]
        public void ShouldRegisterHandler_WhenItAlsoImplementsAnUnrelatedInterface()
        {
            var services = new ServiceCollection();

            services.AddRequestableServices();

            AssertTransientHandler<IRequestHandler<DisposableRequest, string>, DisposableRequestHandler>(services);
        }

        [Fact]
        public void ShouldRegisterEveryHandlerInterface_WhenAClassHandlesMoreThanOneRequest()
        {
            var services = new ServiceCollection();

            services.AddRequestableServices();

            AssertTransientHandler<IRequestHandler<FirstMultiRequest, string>, MultiRequestHandler>(services);
            AssertTransientHandler<IRequestHandler<SecondMultiRequest>, MultiRequestHandler>(services);
        }

        [Fact]
        public void ShouldRegisterHandlerForEachResponseType_WhenARequestHasMoreThanOneResponse()
        {
            var services = new ServiceCollection();

            services.AddRequestableServices();

            AssertTransientHandler<IRequestHandler<DualRequest, int>, DualIntRequestHandler>(services);
            AssertTransientHandler<IRequestHandler<DualRequest, string>, DualStringRequestHandler>(services);
        }

        [Fact]
        public void ShouldSkipAbstractAndOpenGenericHandlers()
        {
            var services = new ServiceCollection();

            services.AddRequestableServices();

            Assert.DoesNotContain(services, d => d.ImplementationType == typeof(AbstractRequestHandler));
            Assert.DoesNotContain(services, d => d.ImplementationType == typeof(GenericRequestHandler<>));
            Assert.DoesNotContain(services, d => d.ServiceType == typeof(IRequestHandler<AbstractRequest, string>));
        }

        [Fact]
        public void ShouldRegisterConfiguredMediatorImplementationAndLifetime()
        {
            var services = new ServiceCollection();

            services.AddRequestableServices(config =>
            {
                config.MediatorImplementationType = typeof(CustomMediator);
                config.MediatorServiceLifetime = ServiceLifetime.Singleton;
            });

            var descriptor = Assert.Single(services, d => d.ServiceType == typeof(IMediator));
            Assert.Equal(typeof(CustomMediator), descriptor.ImplementationType);
            Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
        }

        [Fact]
        public void ShouldThrowArgumentException_WhenMediatorImplementationDoesNotImplementIMediator()
        {
            var services = new ServiceCollection();

            var exception = Assert.Throws<ArgumentException>(() => services.AddRequestableServices(
                config => config.MediatorImplementationType = typeof(string)));

            Assert.StartsWith("'String' does not implement IMediator.", exception.Message);
        }

        [Fact]
        public void ShouldThrowArgumentNullException_WhenConfigureIsNull()
        {
            var services = new ServiceCollection();

            Assert.Throws<ArgumentNullException>(() => services.AddRequestableServices(null!));
        }

        private sealed class CustomMediator : IMediator
        {
            public Task SendAsync(IRequest request, CancellationToken cancellationToken) => Task.CompletedTask;

            public Task<TResponse> SendAsync<TResponse>(
                IRequest<TResponse> request,
                CancellationToken cancellationToken) => Task.FromResult(default(TResponse)!);
        }
    }

    private static void AssertTransientHandler<TService, TImplementation>(IServiceCollection services)
    {
        var descriptor = Assert.Single(services, d => d.ServiceType == typeof(TService));
        Assert.Equal(typeof(TImplementation), descriptor.ImplementationType);
        Assert.Equal(ServiceLifetime.Transient, descriptor.Lifetime);
    }
}