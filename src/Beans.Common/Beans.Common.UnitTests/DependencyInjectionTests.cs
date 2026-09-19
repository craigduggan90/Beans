using Beans.Common.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Beans.Common.UnitTests;

public static class DependencyInjectionTests
{
    public class AddCommonServices
    {
        [Fact]
        public void ShouldRegisterEnvironmentVariableAccessorAsSingleton()
        {
            var services = new ServiceCollection();

            services.AddCommonServices();

            var descriptor = Assert.Single(services, d => d.ServiceType == typeof(IEnvironmentVariableAccessor));
            Assert.Equal(typeof(EnvironmentVariableAccessor), descriptor.ImplementationType);
            Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
        }

        [Fact]
        public void ShouldReturnTheSameServiceCollection()
        {
            var services = new ServiceCollection();

            var result = services.AddCommonServices();

            Assert.Same(services, result);
        }
    }
}