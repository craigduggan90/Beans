using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Beans.Requestable.IntegrationTests.TestSupport;

/// <summary>Builds the HTTP host used to prove the mediator works end to end inside a real ASP.NET Core pipeline.</summary>
internal static class TestHostFactory
{
    public static async Task<WebApplication> CreateHostAsync()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = Environments.Production });
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();

        builder.Services.AddSingleton<MessageLog>();
        builder.Services.AddScoped<RequestScope>();

        builder.Services.AddRequestableServices();

        builder.Services
            .AddControllers()
            .AddApplicationPart(typeof(TestController).Assembly);

        var app = builder.Build();
        app.MapControllers();

        await app.StartAsync();
        return app;
    }
}