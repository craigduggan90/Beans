using Microsoft.AspNetCore.Mvc;

namespace Beans.Patchable.IntegrationTests.TestSupport;

/// <summary>
/// The controller-based equivalent of the minimal API endpoint in <see cref="TestHostFactory"/>, routing
/// through the same <see cref="IWidgetService"/>.
/// </summary>
[ApiController]
[Route("widget")]
public sealed class WidgetController(IWidgetService widgetService) : ControllerBase
{
    [HttpPost]
    public IActionResult Patch(WidgetPatchRequest request)
    {
        widgetService.Patch(request.Name, request.Description, request.Score);
        return NoContent();
    }
}
