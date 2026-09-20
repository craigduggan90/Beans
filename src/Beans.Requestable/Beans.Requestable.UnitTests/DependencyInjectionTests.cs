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

        [Fact]
        public void ShouldReturnTheSameServiceCollection_WhenConfigurationIsProvided()
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

        [Fact]
        public void ShouldThrowRequestableException_WhenAssembliesContainDuplicateHandlers()
        {
            var services = new ServiceCollection();
            var assembly = new FakeAssembly(typeof(FirstDuplicateHandler<int>), typeof(SecondDuplicateHandler<int>));

            Assert.Throws<RequestableException>(
                () => services.AddRequestableServices(config => config.Assemblies = [assembly]));
            Assert.Empty(services);
        }

        [Fact]
        public void ShouldRegisterHandlersFromEveryAssembly_WhenNoneOfThemDuplicateAnother()
        {
            var services = new ServiceCollection();
            var first = new FakeAssembly(typeof(FirstDuplicateHandler<int>));
            var second = new FakeAssembly(typeof(PingRequestHandler));

            services.AddRequestableServices(config => config.Assemblies = [first, second]);

            AssertTransientHandler<IRequestHandler<DuplicateRequest<int>, string>, FirstDuplicateHandler<int>>(services);
            AssertTransientHandler<IRequestHandler<PingRequest>, PingRequestHandler>(services);
        }

        [Fact]
        public async Task ShouldAllowAHandlerRegisteredAfterwardsToReplaceAScannedHandler()
        {
            var services = new ServiceCollection();
            services.AddRequestableServices();
            services.AddTransient<IRequestHandler<EchoRequest, string>, ReplacementEchoRequestHandler<int>>();
            await using var provider = services.BuildServiceProvider();

            var actual = await provider.GetRequiredService<IMediator>()
                .SendAsync(new EchoRequest("hello"), TestContext.Current.CancellationToken);

            Assert.Equal("replaced", actual);
        }

        private sealed class CustomMediator : IMediator
        {
            public Task SendAsync(IRequest request, CancellationToken cancellationToken) => Task.CompletedTask;

            public Task<TResponse> SendAsync<TResponse>(
                IRequest<TResponse> request,
                CancellationToken cancellationToken) => Task.FromResult(default(TResponse)!);
        }
    }

    public class AddHandlers
    {
        [Fact]
        public void ShouldThrowRequestableException_WhenTwoTypesHandleTheSameRequestWithAResponse()
        {
            var services = new ServiceCollection();
            var first = typeof(FirstDuplicateHandler<int>);
            var second = typeof(SecondDuplicateHandler<int>);

            var exception = Assert.Throws<RequestableException>(() => services.AddHandlers([first, second]));

            Assert.Equal(
                "Unable to register request handlers: more than one handler found for " +
                $"'DuplicateRequest`1' request ({first.FullName}, {second.FullName}).",
                exception.Message);
        }

        [Fact]
        public void ShouldThrowRequestableException_WhenTwoTypesHandleTheSameVoidRequest()
        {
            var services = new ServiceCollection();
            var first = typeof(FirstDuplicateVoidHandler<int>);
            var second = typeof(SecondDuplicateVoidHandler<int>);

            var exception = Assert.Throws<RequestableException>(() => services.AddHandlers([first, second]));

            Assert.Equal(
                "Unable to register request handlers: more than one handler found for " +
                $"'DuplicateVoidRequest`1' request ({first.FullName}, {second.FullName}).",
                exception.Message);
        }

        [Fact]
        public void ShouldReportEveryDuplicatedRequestInOneException()
        {
            var services = new ServiceCollection();
            var duplicateTypes = new[]
            {
                typeof(FirstDuplicateVoidHandler<int>),
                typeof(SecondDuplicateVoidHandler<int>),
                typeof(FirstDuplicateHandler<int>),
                typeof(SecondDuplicateHandler<int>)
            };

            var exception = Assert.Throws<RequestableException>(() => services.AddHandlers(duplicateTypes));

            Assert.Equal(
                "Unable to register request handlers: more than one handler found for " +
                $"'DuplicateRequest`1' request ({typeof(FirstDuplicateHandler<int>).FullName}, " +
                $"{typeof(SecondDuplicateHandler<int>).FullName}); " +
                $"'DuplicateVoidRequest`1' request ({typeof(FirstDuplicateVoidHandler<int>).FullName}, " +
                $"{typeof(SecondDuplicateVoidHandler<int>).FullName}).",
                exception.Message);
        }

        [Fact]
        public void ShouldDescribeDuplicatesTheSameWay_RegardlessOfTheOrderTypesAreFound()
        {
            var first = typeof(FirstDuplicateHandler<int>);
            var second = typeof(SecondDuplicateHandler<int>);

            var forwards = Assert.Throws<RequestableException>(
                () => new ServiceCollection().AddHandlers([first, second]));
            var backwards = Assert.Throws<RequestableException>(
                () => new ServiceCollection().AddHandlers([second, first]));

            Assert.Equal(forwards.Message, backwards.Message);
        }

        [Fact]
        public void ShouldRegisterNothing_WhenDuplicatesAreFound()
        {
            var services = new ServiceCollection();

            Assert.Throws<RequestableException>(() => services.AddHandlers(
                [typeof(PingRequestHandler), typeof(FirstDuplicateHandler<int>), typeof(SecondDuplicateHandler<int>)]));

            Assert.Empty(services);
        }

        [Fact]
        public void ShouldRegisterTypeOnce_WhenItIsListedMoreThanOnce()
        {
            var services = new ServiceCollection();

            services.AddHandlers([typeof(PingRequestHandler), typeof(PingRequestHandler)]);

            AssertTransientHandler<IRequestHandler<PingRequest>, PingRequestHandler>(services);
        }

        [Fact]
        public void ShouldNotTreatHandlersForDifferentResponsesAsDuplicates()
        {
            var services = new ServiceCollection();

            services.AddHandlers([typeof(DualIntRequestHandler), typeof(DualStringRequestHandler)]);

            AssertTransientHandler<IRequestHandler<DualRequest, int>, DualIntRequestHandler>(services);
            AssertTransientHandler<IRequestHandler<DualRequest, string>, DualStringRequestHandler>(services);
        }
    }

    private static void AssertTransientHandler<TService, TImplementation>(IServiceCollection services)
    {
        var descriptor = Assert.Single(services, d => d.ServiceType == typeof(TService));
        Assert.Equal(typeof(TImplementation), descriptor.ImplementationType);
        Assert.Equal(ServiceLifetime.Transient, descriptor.Lifetime);
    }
}