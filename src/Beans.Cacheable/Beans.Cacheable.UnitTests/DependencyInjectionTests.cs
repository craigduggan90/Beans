using Microsoft.Extensions.DependencyInjection;

namespace Beans.Cacheable.UnitTests;

public static class DependencyInjectionTests
{
    public class AddCacheableServices
    {
        [Fact]
        public void ShouldRegisterDistributedCacheClientAsSingleton()
        {
            var services = new ServiceCollection();

            services.AddCacheableServices();

            var descriptor = Assert.Single(services, d => d.ServiceType == typeof(IDistributedCacheClient));
            Assert.Equal(typeof(DistributedCacheClient), descriptor.ImplementationType);
            Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
        }

        [Fact]
        public void ShouldReturnTheSameServiceCollection()
        {
            var services = new ServiceCollection();

            var result = services.AddCacheableServices();

            Assert.Same(services, result);
        }
    }
}