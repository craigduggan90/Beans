namespace Beans.Patchable.IntegrationTests.TestSupport;

/// <summary>
/// A <see cref="PatchableEntity"/> used to exercise patching end-to-end through an HTTP host.
/// </summary>
public sealed class Widget : PatchableEntity
{
    public string Name { get; set; } = "Original";

    public string? Description { get; set; } = "Original description";

    public int? Score { get; set; } = 10;
}