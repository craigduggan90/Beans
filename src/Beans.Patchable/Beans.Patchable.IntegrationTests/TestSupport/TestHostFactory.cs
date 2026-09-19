using Beans.Patchable.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Beans.Patchable.IntegrationTests.TestSupport;

/// <summary>
/// Builds the two HTTP hosts used to prove <see cref="OptionalJsonConverterFactory"/> and
/// <see cref="IWidgetService"/> behave identically regardless of whether the JSON body is bound by a
/// minimal API endpoint or an MVC controller.
/// </summary>
internal static class TestHostFactory
{
    public static async Task<WebApplication> CreateMinimalApiHostAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();

        builder.Services.AddSingleton<IWidgetService, WidgetService>();
        builder.Services.ConfigureHttpJsonOptions(
            options => options.SerializerOptions.Converters.Add(new OptionalJsonConverterFactory()));

        var app = builder.Build();

        app.MapPost("/widget", (WidgetPatchRequest request, IWidgetService widgetService) =>
        {
            widgetService.Patch(request.Name, request.Description, request.Score);
            return Results.NoContent();
        });

        await app.StartAsync();
        return app;
    }

    public static async Task<WebApplication> CreateControllerHostAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();

        builder.Services.AddSingleton<IWidgetService, WidgetService>();
        builder.Services
            .AddControllers()
            .AddApplicationPart(typeof(WidgetController).Assembly)
            .AddJsonOptions(
                options => options.JsonSerializerOptions.Converters.Add(new OptionalJsonConverterFactory()));

        var app = builder.Build();
        app.MapControllers();

        await app.StartAsync();
        return app;
    }
}