namespace Beans.Patchable.IntegrationTests.TestSupport;

/// <summary>
/// The single piece of business logic shared by both HTTP hosts, so a test that passes for one host
/// exercises exactly the same patching behaviour as the other.
/// </summary>
public interface IWidgetService
{
    Widget Widget { get; }

    void Patch(Optional<string> name, Optional<string?> description, Optional<int?> score);
}

/// <summary>
/// A singleton service wrapping a single <see cref="Widget"/> instance, so both test hosts can patch it
/// over HTTP and the test can then inspect its state directly.
/// </summary>
public sealed class WidgetService : IWidgetService
{
    public Widget Widget { get; } = new();

    public void Patch(Optional<string> name, Optional<string?> description, Optional<int?> score)
    {
        Widget.UpdateProperty(nameof(Widget.Name), name);
        Widget.UpdateProperty(nameof(Widget.Description), description);
        Widget.UpdateProperty(nameof(Widget.Score), score);
    }
}