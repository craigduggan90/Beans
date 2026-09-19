using Beans.Patchable.IntegrationTests.TestSupport;
using Microsoft.AspNetCore.Builder;

namespace Beans.Patchable.IntegrationTests;

public sealed class MinimalApiWidgetPatchEndpointTests : WidgetPatchEndpointTestsBase
{
    protected override Task<WebApplication> CreateAppAsync() => TestHostFactory.CreateMinimalApiHostAsync();
}