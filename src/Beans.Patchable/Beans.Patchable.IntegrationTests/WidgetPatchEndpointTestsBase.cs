using Beans.Patchable.IntegrationTests.TestSupport;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text;

namespace Beans.Patchable.IntegrationTests;

/// <summary>
/// Patch behaviour that must hold regardless of which HTTP host receives the request. Concrete subclasses
/// supply the host; the JSON on the wire and the assertions against <see cref="Widget"/> are shared, so a
/// divergence between the minimal API and controller pipelines shows up as a failure in exactly one subclass.
/// </summary>
public abstract class WidgetPatchEndpointTestsBase : IAsyncLifetime
{
    private WebApplication _app = null!;
    private HttpClient _client = null!;

    protected abstract Task<WebApplication> CreateAppAsync();

    public async ValueTask InitializeAsync()
    {
        _app = await CreateAppAsync();
        _client = _app.GetTestClient();
    }

    public async ValueTask DisposeAsync()
    {
        _client.Dispose();
        await _app.DisposeAsync();
    }

    private Widget Widget => _app.Services.GetRequiredService<IWidgetService>().Widget;

    private Task<HttpResponseMessage> PatchAsync(string json) =>
        _client.PostAsync(
            "/widget",
            new StringContent(json, Encoding.UTF8, "application/json"),
            TestContext.Current.CancellationToken);

    [Fact]
    public async Task WhenOnlyNameIsProvided_UpdatesNameAndLeavesOtherPropertiesUnchanged()
    {
        var response = await PatchAsync("""{"Name":"Updated"}""");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal("Updated", Widget.Name);
        Assert.Equal("Original description", Widget.Description);
        Assert.Equal(10, Widget.Score);
        Assert.True(Widget.IsDirty);
    }

    [Fact]
    public async Task WhenNoPropertiesAreProvided_LeavesEntityUnchangedAndNotDirty()
    {
        var response = await PatchAsync("{}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal("Original", Widget.Name);
        Assert.Equal("Original description", Widget.Description);
        Assert.Equal(10, Widget.Score);
        Assert.False(Widget.IsDirty);
    }

    [Fact]
    public async Task WhenDescriptionIsExplicitlyNull_SetsDescriptionToNull()
    {
        var response = await PatchAsync("""{"Description":null}""");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Null(Widget.Description);
        Assert.Equal("Original", Widget.Name);
        Assert.True(Widget.IsDirty);
    }

    [Fact]
    public async Task WhenScoreIsExplicitlyNull_SetsScoreToNull()
    {
        var response = await PatchAsync("""{"Score":null}""");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Null(Widget.Score);
        Assert.True(Widget.IsDirty);
    }

    [Fact]
    public async Task WhenAllPropertiesAreProvided_UpdatesAllProperties()
    {
        var response = await PatchAsync("""{"Name":"Updated","Description":"Updated description","Score":99}""");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal("Updated", Widget.Name);
        Assert.Equal("Updated description", Widget.Description);
        Assert.Equal(99, Widget.Score);
        Assert.True(Widget.IsDirty);
    }

    [Fact]
    public async Task WhenProvidedValueMatchesCurrentValue_DoesNotMarkEntityDirty()
    {
        var response = await PatchAsync("""{"Name":"Original"}""");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal("Original", Widget.Name);
        Assert.False(Widget.IsDirty);
    }
}