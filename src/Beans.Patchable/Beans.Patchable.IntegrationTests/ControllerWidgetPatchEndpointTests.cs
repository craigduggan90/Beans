using Beans.Patchable.IntegrationTests.TestSupport;
using Microsoft.AspNetCore.Builder;

namespace Beans.Patchable.IntegrationTests;

public sealed class ControllerWidgetPatchEndpointTests : WidgetPatchEndpointTestsBase
{
    protected override Task<WebApplication> CreateAppAsync() => TestHostFactory.CreateControllerHostAsync();
}
