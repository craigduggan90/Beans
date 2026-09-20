using Beans.Requestable.IntegrationTests.TestSupport;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace Beans.Requestable.IntegrationTests;

public sealed class MediatorEndpointTests : IAsyncLifetime
{
    private WebApplication _app = null!;
    private HttpClient _client = null!;

    public async ValueTask InitializeAsync()
    {
        _app = await TestHostFactory.CreateHostAsync();
        _client = _app.GetTestClient();
    }

    public async ValueTask DisposeAsync()
    {
        _client.Dispose();
        await _app.DisposeAsync();
    }

    [Fact]
    public async Task WhenRequestHasAResponse_ReturnsTheHandlerResponse()
    {
        var response = await _client.GetAsync("/test/greet/Beans", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Hello, Beans!", await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task WhenVoidRequestIsSent_InvokesTheHandler()
    {
        var response = await _client.PostAsJsonAsync(
            "/test/log",
            new LogModel("logged"),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(["logged"], _app.Services.GetRequiredService<MessageLog>().Messages);
    }

    [Fact]
    public async Task WhenHandlerNeedsAScopedService_ItSharesTheScopeOfTheRequest()
    {
        var result = await _client.GetFromJsonAsync<ScopeResult>(
            "/test/scope",
            TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(result.Controller, result.Handler);
    }

    [Fact]
    public async Task WhenSeparateRequestsAreMade_EachGetsItsOwnScope()
    {
        var first = await _client.GetFromJsonAsync<ScopeResult>(
            "/test/scope",
            TestContext.Current.CancellationToken);
        var second = await _client.GetFromJsonAsync<ScopeResult>(
            "/test/scope",
            TestContext.Current.CancellationToken);

        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.NotEqual(first.Handler, second.Handler);
    }

    [Fact]
    public async Task WhenNoHandlerIsRegistered_ThrowsRequestHandlerException()
    {
        var exception = await Assert.ThrowsAsync<RequestHandlerException>(
            () => _client.GetAsync("/test/unhandled", TestContext.Current.CancellationToken));

        Assert.Equal("Unable to resolve handler for 'UnhandledRequest' request.", exception.Message);
    }

    [Fact]
    public async Task WhenHandlerThrows_TheExceptionPropagates()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _client.GetAsync("/test/fail", TestContext.Current.CancellationToken));

        Assert.Equal("handler failed", exception.Message);
    }
}