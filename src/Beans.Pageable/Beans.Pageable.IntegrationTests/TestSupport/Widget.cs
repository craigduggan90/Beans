using Beans.Common.Providers;

namespace Beans.Pageable.IntegrationTests.TestSupport;

/// <summary>An entity shaped like the one in the README: get-only cursor and creation date.</summary>
public class Widget : IHasCursor, IHasCreatedTimestamp, IHasModifiedTimestamp
{
    public int Id { get; private set; }

    public string Name { get; set; } = string.Empty;

    public long Cursor { get; } = CursorGenerator.Next();

    public DateTime DateCreated { get; } = DateTimeOffsetProvider.UtcNow.UtcDateTime;

    public DateTime DateModified { get; private set; } = DateTimeOffsetProvider.UtcNow.UtcDateTime;

    public void Touch() => DateModified = DateTimeOffsetProvider.UtcNow.UtcDateTime;
}