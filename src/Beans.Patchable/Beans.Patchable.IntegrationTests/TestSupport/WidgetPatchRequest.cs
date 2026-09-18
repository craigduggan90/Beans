namespace Beans.Patchable.IntegrationTests.TestSupport;

/// <summary>
/// The JSON request body for patching a <see cref="Widget"/>, bound identically by both the minimal API
/// endpoint and the MVC controller used in these tests.
/// </summary>
public sealed record WidgetPatchRequest(
    Optional<string> Name,
    Optional<string?> Description,
    Optional<int?> Score);
